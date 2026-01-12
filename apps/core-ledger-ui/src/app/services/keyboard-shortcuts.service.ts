import { Injectable, inject, signal, computed, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { fromEvent, filter } from 'rxjs';
import { ENVIRONMENT } from '../config/environment.config';
import { ToastService } from './toast-service';

/** Definição de atalho de teclado */
export interface KeyboardShortcut {
  /** Identificador único para o atalho */
  id: string;
  /** Rótulo de exibição para o atalho */
  label: string;
  /** Combinação de teclas (ex: 'alt+n', 'ctrl+shift+s') */
  keys: string;
  /** Função de retorno quando o atalho é acionado */
  callback: () => void;
  /** Categoria para agrupamento em diálogo de ajuda */
  category?: string;
  /** Se o atalho está ativado no momento */
  enabled?: boolean;
}

/** Combinação de teclas analisada */
interface ParsedKeys {
  ctrl: boolean;
  alt: boolean;
  shift: boolean;
  meta: boolean;
  key: string;
}

/**
 * Serviço global de atalhos de teclado.
 *
 * Fornece uma forma centralizada de registrar e gerenciar atalhos de teclado
 * em toda a aplicação. Suporta teclas modificadoras (Ctrl, Alt, Shift, Meta).
 * Detecta automaticamente a plataforma e mostra os símbolos de tecla apropriados.
 *
 * @example
 * ```typescript
 * // Em um componente
 * private readonly keyboardService = inject(KeyboardShortcutsService);
 *
 * ngOnInit() {
 *   this.keyboardService.register({
 *     id: 'create-item',
 *     label: 'Criar novo item',
 *     keys: 'alt+n',
 *     callback: () => this.router.navigate(['/items/new']),
 *     category: 'Navegação'
 *   });
 * }
 * ```
 */
@Injectable({ providedIn: 'root' })
export class KeyboardShortcutsService {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly environment = inject(ENVIRONMENT);
  private readonly toastService = inject(ToastService);

  /** Se o usuário está no macOS */
  private readonly isMac = typeof navigator !== 'undefined' && /Mac|iPod|iPhone|iPad/.test(navigator.platform);

  /** Se estamos em modo de desenvolvimento (mostrar toasts de depuração) */
  private readonly isDevelopment = !this.environment.production;

  /** Atalhos registrados */
  private readonly _shortcuts = signal<Map<string, KeyboardShortcut>>(new Map());

  /** Lista pública somente leitura de atalhos */
  readonly shortcuts = computed(() => Array.from(this._shortcuts().values()));

  /** Atalhos agrupados por categoria */
  readonly shortcutsByCategory = computed(() => {
    const groups = new Map<string, KeyboardShortcut[]>();
    for (const shortcut of this.shortcuts()) {
      const category = shortcut.category || 'Geral';
      if (!groups.has(category)) {
        groups.set(category, []);
      }
      groups.get(category)!.push(shortcut);
    }
    return groups;
  });

  /** Se os atalhos estão ativados no momento (desativado durante foco em modal/formulário) */
  private readonly _enabled = signal(true);
  readonly enabled = this._enabled.asReadonly();

  constructor() {
    // Ouvir eventos keydown globais
    fromEvent<KeyboardEvent>(document, 'keydown')
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter(() => this._enabled()),
        filter((event) => this.shouldHandleEvent(event))
      )
      .subscribe((event) => this.handleKeydown(event));

    // Registrar atalhos globais padrão
    this.registerDefaults();
  }

  /**
   * Registra um atalho de teclado
   */
  register(shortcut: KeyboardShortcut): void {
    this._shortcuts.update((shortcuts) => {
      const updated = new Map(shortcuts);
      updated.set(shortcut.id, { enabled: true, ...shortcut });
      return updated;
    });
  }

  /**
   * Desregistra um atalho de teclado
   */
  unregister(id: string): void {
    this._shortcuts.update((shortcuts) => {
      const updated = new Map(shortcuts);
      updated.delete(id);
      return updated;
    });
  }

  /**
   * Ativa ou desativa um atalho específico
   */
  setEnabled(id: string, enabled: boolean): void {
    this._shortcuts.update((shortcuts) => {
      const shortcut = shortcuts.get(id);
      if (shortcut) {
        const updated = new Map(shortcuts);
        updated.set(id, { ...shortcut, enabled });
        return updated;
      }
      return shortcuts;
    });
  }

  /**
   * Ativa todos os atalhos
   */
  enableAll(): void {
    this._enabled.set(true);
  }

  /**
   * Desativa todos os atalhos (ex: quando modal está aberto)
   */
  disableAll(): void {
    this._enabled.set(false);
  }

  /**
   * Obtém rótulo de combinação de tecla legível (ciente de plataforma)
   */
  getKeyLabel(keys: string): string {
    return this.getKeyParts(keys).join(this.isMac ? '' : '+');
  }

  /**
   * Obtém partes da tecla como array para exibição (ciente de plataforma)
   */
  getKeyParts(keys: string): string[] {
    const parsed = this.parseKeys(keys);
    const parts: string[] = [];

    if (this.isMac) {
      // macOS usa símbolos: ⌃ Control, ⌥ Option, ⇧ Shift, ⌘ Command
      if (parsed.ctrl) parts.push('⌃');
      if (parsed.alt) parts.push('⌥');
      if (parsed.shift) parts.push('⇧');
      if (parsed.meta) parts.push('⌘');
    } else {
      // Windows/Linux usa rótulos de texto
      if (parsed.ctrl) parts.push('Ctrl');
      if (parsed.alt) parts.push('Alt');
      if (parsed.shift) parts.push('Shift');
      if (parsed.meta) parts.push('Win');
    }

    // Maiúscula em teclas de letra única, formatar teclas especiais
    let keyLabel = parsed.key;
    if (keyLabel.length === 1) {
      keyLabel = keyLabel.toUpperCase();
    } else if (keyLabel === '/') {
      keyLabel = '?';
    } else {
      // Maiúscula primeira letra para teclas nomeadas
      keyLabel = keyLabel.charAt(0).toUpperCase() + keyLabel.slice(1);
    }
    parts.push(keyLabel);

    return parts;
  }

  /**
   * Verifica se está rodando no macOS
   */
  get isMacPlatform(): boolean {
    return this.isMac;
  }

  /**
   * Registra atalhos padrão em toda a aplicação
   */
  private registerDefaults(): void {
    // Atalho de ajuda (mostrar atalhos de teclado)
    this.register({
      id: 'show-help',
      label: 'Mostrar atalhos de teclado',
      keys: 'shift+?',
      callback: () => this.showHelp(),
      category: 'Ajuda',
    });

    // Atalhos de navegação
    this.register({
      id: 'go-dashboard',
      label: 'Ir para Painel',
      keys: 'alt+d',
      callback: () => this.router.navigate(['/dashboard']),
      category: 'Navegação',
    });

    // Atalhos de foco
    this.register({
      id: 'focus-search',
      label: 'Foco em pesquisa',
      keys: 'ctrl+k',
      callback: () => this.focusSearch(),
      category: 'Foco',
    });

    this.register({
      id: 'focus-main',
      label: 'Foco em conteúdo principal',
      keys: 'alt+m',
      callback: () => this.focusMain(),
      category: 'Foco',
    });
  }

  /**
   * Trata evento keydown e dispara atalho correspondente
   */
  private handleKeydown(event: KeyboardEvent): void {
    const shortcuts = this._shortcuts();

    for (const shortcut of shortcuts.values()) {
      if (!shortcut.enabled) continue;

      const parsed = this.parseKeys(shortcut.keys);
      if (this.matchesEvent(event, parsed)) {
        event.preventDefault();
        event.stopPropagation();

        // Mostrar notificação toast em modo de desenvolvimento
        if (this.isDevelopment) {
          const keyLabel = this.getKeyLabel(shortcut.keys);
          this.toastService.info(
            `[ATALHO] ${keyLabel} → ${shortcut.label}`,
            2000 // Atraso curto de 2 segundos para feedback de teclado
          );
        }

        shortcut.callback();
        return;
      }
    }
  }

  /**
   * Analisa string de combinação de teclas em componentes
   */
  private parseKeys(keys: string): ParsedKeys {
    const parts = keys.toLowerCase().split('+');
    const key = parts.pop() || '';

    return {
      ctrl: parts.includes('ctrl') || parts.includes('control'),
      alt: parts.includes('alt'),
      shift: parts.includes('shift'),
      meta: parts.includes('meta') || parts.includes('cmd'),
      key: key === '?' ? '/' : key, // Tratar ? como Shift+/
    };
  }

  /**
   * Verifica se evento de teclado corresponde à combinação de teclas analisada
   */
  private matchesEvent(event: KeyboardEvent, parsed: ParsedKeys): boolean {
    // Verificar modificadores
    if (event.ctrlKey !== parsed.ctrl) return false;
    if (event.altKey !== parsed.alt) return false;
    if (event.shiftKey !== parsed.shift) return false;
    if (event.metaKey !== parsed.meta) return false;

    const targetKey = parsed.key.toLowerCase();

    // Para teclas de letra, sempre usar event.code (funciona em todas as plataformas)
    // Isso trata macOS Option+tecla produzindo caracteres especiais
    if (targetKey.length === 1 && targetKey >= 'a' && targetKey <= 'z') {
      const expectedCode = `Key${targetKey.toUpperCase()}`;
      return event.code === expectedCode;
    }

    // Para teclas especiais, verificar event.key
    const eventKey = event.key.toLowerCase();

    // Tratar ? que requer Shift+/
    if (targetKey === '/' || targetKey === '?') {
      return eventKey === '/' || eventKey === '?';
    }

    return eventKey === targetKey;
  }

  /**
   * Verifica se o evento deve ser tratado (não em input/textarea)
   */
  private shouldHandleEvent(event: KeyboardEvent): boolean {
    const target = event.target as HTMLElement;
    const tagName = target.tagName.toLowerCase();

    // Não tratar atalhos ao digitar em campos de formulário
    // a menos que seja uma combinação de tecla modificadora
    if (tagName === 'input' || tagName === 'textarea' || target.isContentEditable) {
      // Permitir combinações Ctrl/Alt/Meta em inputs
      return event.ctrlKey || event.altKey || event.metaKey;
    }

    return true;
  }

  /**
   * Mostrar ajuda de atalhos de teclado
   */
  private showHelp(): void {
    // Por enquanto, registrar no console. Poderia ser melhorado para mostrar um modal.
    console.group('Atalhos de Teclado');
    for (const [category, shortcuts] of this.shortcutsByCategory()) {
      console.group(category);
      for (const shortcut of shortcuts) {
        console.log(`${this.getKeyLabel(shortcut.keys)}: ${shortcut.label}`);
      }
      console.groupEnd();
    }
    console.groupEnd();
  }

  /**
   * Foca a entrada de pesquisa
   */
  private focusSearch(): void {
    const searchInput = document.querySelector<HTMLInputElement>(
      '.sidenav-search__input, [aria-label*="search" i], [placeholder*="search" i]'
    );
    searchInput?.focus();
  }

  /**
   * Foca a área de conteúdo principal
   */
  private focusMain(): void {
    const mainContent = document.getElementById('main-content');
    mainContent?.focus();
  }
}
