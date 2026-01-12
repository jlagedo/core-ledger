import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  output,
  signal,
  viewChild,
  viewChildren,
  QueryList,
  afterNextRender,
  Injector,
} from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { NgbCollapse, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { UserProfile } from '../user-profile/user-profile';
import { DeadlineTicker } from '../deadline-ticker/deadline-ticker';
import { MenuService } from '../../services/menu-service';
import { SidenavStore } from './sidenav-store';
import { MenuItem } from '../../models/menu-item.model';
import { map } from 'rxjs';

@Component({
  selector: 'app-sidenav',
  imports: [RouterModule, NgbCollapse, NgbTooltip, UserProfile, DeadlineTicker],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(keydown)': 'onKeydown($event)',
  },
})
export class Sidenav {
  private readonly menuService = inject(MenuService);
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);
  readonly store = inject(SidenavStore);

  readonly menuItems = this.menuService.menuItems;
  sidenavToggle = output<boolean>();

  // Rastrear URL atual para detecção de estado ativo do pai
  private readonly currentUrl = toSignal(
    this.router.events.pipe(map(() => this.router.url)),
    { initialValue: this.router.url }
  );

  // Funcionalidade de pesquisa
  readonly searchQuery = signal('');
  readonly isSearchFocused = signal(false);
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');

  // Navegação por teclado
  /** Referência ao elemento de navegação para navegação por teclado */
  private readonly navElement = viewChild<ElementRef<HTMLElement>>('navElement');

  /** Índice do item de menu atualmente focado (para navegação por teclado) */
  private readonly focusedItemIndex = signal<number>(-1);

  // Itens de menu filtrados baseado em consulta de pesquisa
  readonly filteredMenuItems = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const items = this.menuItems();

    if (!query) return items;

    return items
      .map(item => {
        // Verificar se o rótulo do pai corresponde
        const parentMatches = item.label.toLowerCase().includes(query);

        // Verificar se algum filho corresponde
        const matchingChildren = item.children?.filter(child =>
          child.label.toLowerCase().includes(query)
        );

        // Incluir item se pai corresponder OU tiver filhos correspondentes
        if (parentMatches) {
          return item; // Retornar item completo com todos os filhos
        } else if (matchingChildren && matchingChildren.length > 0) {
          return { ...item, children: matchingChildren }; // Retornar com filhos filtrados
        }
        return null;
      })
      .filter((item): item is MenuItem => item !== null);
  });

  // Verificar se a pesquisa tem resultados
  readonly hasSearchResults = computed(() => this.filteredMenuItems().length > 0);

  // Verificar se estamos pesquisando ativamente
  readonly isSearching = computed(() => this.searchQuery().trim().length > 0);

  // Determinar se um submenu deve ser mostrado expandido
  // Ao pesquisar: sempre expandir para mostrar filhos correspondentes
  // Quando não está pesquisando: usar o estado de recolhimento armazenado
  shouldShowExpanded(itemLabel: string): boolean {
    if (this.isSearching()) {
      return true; // Sempre expandir ao pesquisar
    }
    return !this.store.isMenuItemCollapsed(itemLabel);
  }

  // Estado do menu flutuante - armazena dados completos do item para renderizar fora do contêiner de rolagem
  readonly flyoutTop = signal<number | null>(null);
  readonly activeFlyoutItem = signal<MenuItem | null>(null);
  private hideTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // Inicializar estado recolhido para todos os itens de menu com filhos
    const menuLabelsWithChildren = this.menuItems()
      .filter((item) => item.children && item.children.length > 0)
      .map((item) => item.label);

    this.store.initializeMenuItems(menuLabelsWithChildren);
  }

  toggleSidenav() {
    this.store.toggleSidenav();
    this.sidenavToggle.emit(this.store.isSidenavCollapsed());
  }

  showFlyout(event: MouseEvent, item: MenuItem) {
    // Cancelar qualquer ocultação pendente
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }

    const target = event.currentTarget as HTMLElement;
    // Obter posição relativa ao sidenav-wrapper (ancestral posicionado)
    const wrapper = target.closest('.sidenav-wrapper') as HTMLElement;
    const wrapperRect = wrapper?.getBoundingClientRect();
    const targetRect = target.getBoundingClientRect();
    // Calcular topo relativo ao wrapper
    const relativeTop = targetRect.top - (wrapperRect?.top ?? 0);
    this.flyoutTop.set(relativeTop);
    this.activeFlyoutItem.set(item);
  }

  hideFlyout() {
    // Adicionar um pequeno atraso antes de ocultar para permitir que o mouse chegue ao menu flutuante
    this.hideTimeout = setTimeout(() => {
      this.flyoutTop.set(null);
      this.activeFlyoutItem.set(null);
      this.hideTimeout = null;
    }, 100);
  }

  // Manter menu flutuante visível - cancela qualquer timeout de ocultação pendente
  keepFlyoutVisible() {
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }
  }

  // Verificar se um item de menu pai tem alguma rota filha ativa
  isParentActive(item: MenuItem): boolean {
    if (!item.children) return false;
    const url = this.currentUrl();
    return item.children.some((child) => {
      if (!child.route) return false;
      // Verificar se a URL atual começa com a rota do filho
      return url.startsWith(child.route);
    });
  }

  // Métodos de pesquisa
  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  clearSearch() {
    this.searchQuery.set('');
    this.searchInput()?.nativeElement.focus();
  }

  onSearchKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      this.clearSearch();
      this.searchInput()?.nativeElement.blur();
    }
  }

  // Destacar texto correspondente nos rótulos
  highlightMatch(text: string): string {
    const query = this.searchQuery().trim();
    if (!query) return text;

    const regex = new RegExp(`(${this.escapeRegex(query)})`, 'gi');
    return text.replace(regex, '<mark class="search-highlight">$1</mark>');
  }

  private escapeRegex(str: string): string {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  // ============================================================
  // NAVEGAÇÃO POR TECLADO
  // ============================================================

  /**
   * Lidar com eventos de teclado para navegação de menu.
   * Oferece suporte a teclas de seta, Enter, Espaço, Home, End e Escape.
   */
  onKeydown(event: KeyboardEvent): void {
    const target = event.target as HTMLElement;

    // Não interferir com a digitação de entrada de pesquisa
    if (target.matches('input')) {
      return;
    }

    const navLinks = this.getNavigableElements();
    if (navLinks.length === 0) return;

    const currentIndex = this.focusedItemIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.focusNextItem(navLinks, currentIndex);
        break;

      case 'ArrowUp':
        event.preventDefault();
        this.focusPreviousItem(navLinks, currentIndex);
        break;

      case 'ArrowRight':
        // Expandir submenu ou mostrar menu flutuante
        if (this.store.isSidenavCollapsed()) {
          this.handleFlyoutOpen(event, navLinks, currentIndex);
        } else {
          this.handleExpandSubmenu(navLinks, currentIndex);
        }
        break;

      case 'ArrowLeft':
        // Recolher submenu ou fechar menu flutuante
        if (this.store.isSidenavCollapsed()) {
          this.handleFlyoutClose();
        } else {
          this.handleCollapseSubmenu(navLinks, currentIndex);
        }
        break;

      case 'Home':
        event.preventDefault();
        this.focusFirstItem(navLinks);
        break;

      case 'End':
        event.preventDefault();
        this.focusLastItem(navLinks);
        break;

      case 'Enter':
      case ' ':
        event.preventDefault();
        this.activateItem(navLinks, currentIndex);
        break;

      case 'Escape':
        // Fechar menu flutuante se aberto, caso contrário desfocar
        if (this.activeFlyoutItem()) {
          this.handleFlyoutClose();
        } else {
          (document.activeElement as HTMLElement)?.blur();
        }
        break;
    }
  }

  /**
   * Obter todos os elementos navegáveis no menu (links e botões pai)
   */
  private getNavigableElements(): HTMLElement[] {
    const nav = this.navElement()?.nativeElement;
    if (!nav) return [];

    return Array.from(
      nav.querySelectorAll<HTMLElement>(
        '.nav-link:not(.btn-toggle-nav .nav-link), .nav-flyout__link'
      )
    ).filter((el) => this.isVisible(el));
  }

  /**
   * Verificar se o elemento está visível
   */
  private isVisible(el: HTMLElement): boolean {
    return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length);
  }

  /**
   * Focar o próximo item de menu
   */
  private focusNextItem(navLinks: HTMLElement[], currentIndex: number): void {
    const nextIndex = currentIndex < navLinks.length - 1 ? currentIndex + 1 : 0;
    this.focusItem(navLinks, nextIndex);
  }

  /**
   * Focar o item de menu anterior
   */
  private focusPreviousItem(navLinks: HTMLElement[], currentIndex: number): void {
    const prevIndex = currentIndex > 0 ? currentIndex - 1 : navLinks.length - 1;
    this.focusItem(navLinks, prevIndex);
  }

  /**
   * Focar o primeiro item de menu
   */
  private focusFirstItem(navLinks: HTMLElement[]): void {
    this.focusItem(navLinks, 0);
  }

  /**
   * Focar o último item de menu
   */
  private focusLastItem(navLinks: HTMLElement[]): void {
    this.focusItem(navLinks, navLinks.length - 1);
  }

  /**
   * Focar um item de menu específico
   */
  private focusItem(navLinks: HTMLElement[], index: number): void {
    if (index >= 0 && index < navLinks.length) {
      navLinks[index].focus();
      this.focusedItemIndex.set(index);
    }
  }

  /**
   * Ativar o item focado (clicar ou navegar)
   */
  private activateItem(navLinks: HTMLElement[], currentIndex: number): void {
    if (currentIndex >= 0 && currentIndex < navLinks.length) {
      const element = navLinks[currentIndex];

      // Verificar se é um menu pai com filhos (possui aria-expanded)
      const isParent = element.hasAttribute('aria-expanded');
      if (isParent) {
        // Alternar o submenu
        element.click();
      } else {
        // Navegar para a rota
        element.click();
      }
    }
  }

  /**
   * Lidar com a expansão de submenu com seta para a direita
   */
  private handleExpandSubmenu(navLinks: HTMLElement[], currentIndex: number): void {
    if (currentIndex >= 0 && currentIndex < navLinks.length) {
      const element = navLinks[currentIndex];
      const isParent = element.hasAttribute('aria-expanded');

      if (isParent && element.getAttribute('aria-expanded') === 'false') {
        element.click(); // Expandir o submenu
      } else if (isParent) {
        // Já expandido, focar no primeiro filho
        afterNextRender(
          () => {
            const submenuId = element.getAttribute('aria-controls');
            if (submenuId) {
              const submenu = document.getElementById(submenuId);
              const firstChild = submenu?.querySelector<HTMLElement>('.nav-link');
              if (firstChild) {
                firstChild.focus();
              }
            }
          },
          { injector: this.injector }
        );
      }
    }
  }

  /**
   * Lidar com o recolhimento de submenu com seta para a esquerda
   */
  private handleCollapseSubmenu(navLinks: HTMLElement[], currentIndex: number): void {
    if (currentIndex >= 0 && currentIndex < navLinks.length) {
      const element = navLinks[currentIndex];
      const isParent = element.hasAttribute('aria-expanded');

      // Se estamos em um submenu, focar no pai
      const submenuParent = element.closest('.btn-toggle-nav');
      if (submenuParent) {
        const parentLink = submenuParent.previousElementSibling as HTMLElement;
        if (parentLink?.classList.contains('nav-link')) {
          parentLink.focus();
          return;
        }
      }

      // Se em um pai expandido, recolhê-lo
      if (isParent && element.getAttribute('aria-expanded') === 'true') {
        element.click();
      }
    }
  }

  /**
   * Lidar com abertura de menu flutuante com teclado (modo recolhido)
   */
  private handleFlyoutOpen(event: KeyboardEvent, navLinks: HTMLElement[], currentIndex: number): void {
    if (currentIndex >= 0 && currentIndex < navLinks.length) {
      const element = navLinks[currentIndex];
      const menuItem = this.getMenuItemFromElement(element);

      if (menuItem?.children) {
        // Simulate mouse enter to show flyout
        const rect = element.getBoundingClientRect();
        const wrapper = element.closest('.sidenav-wrapper') as HTMLElement;
        const wrapperRect = wrapper?.getBoundingClientRect();
        const relativeTop = rect.top - (wrapperRect?.top ?? 0);

        this.flyoutTop.set(relativeTop);
        this.activeFlyoutItem.set(menuItem);

        // Focar no primeiro item do menu flutuante após renderização
        afterNextRender(
          () => {
            const flyout = document.querySelector('.nav-flyout');
            const firstLink = flyout?.querySelector<HTMLElement>('.nav-flyout__link');
            if (firstLink) {
              firstLink.focus();
            }
          },
          { injector: this.injector }
        );
      }
    }
  }

  /**
   * Lidar com fechamento de menu flutuante com teclado
   */
  private handleFlyoutClose(): void {
    this.flyoutTop.set(null);
    this.activeFlyoutItem.set(null);
  }

  /**
   * Obter dados do item de menu a partir do elemento DOM
   */
  private getMenuItemFromElement(element: HTMLElement): MenuItem | null {
    const items = this.filteredMenuItems();
    const parentItems = items.filter((item) => item.children && item.children.length > 0);
    const allParentLinks = element.closest('.sidenav-nav')?.querySelectorAll('.nav-item--parent > .nav-link');
    if (!allParentLinks) return null;

    const index = Array.from(allParentLinks).indexOf(element);
    return index >= 0 && index < parentItems.length ? parentItems[index] : null;
  }

  /**
   * Mostrar menu flutuante a partir de evento de teclado (Enter/Espaço)
   */
  showFlyoutFromKeyboard(item: MenuItem, event: Event): void {
    event.preventDefault();
    const target = event.target as HTMLElement;
    const wrapper = target.closest('.sidenav-wrapper') as HTMLElement;
    const wrapperRect = wrapper?.getBoundingClientRect();
    const targetRect = target.getBoundingClientRect();
    const relativeTop = targetRect.top - (wrapperRect?.top ?? 0);

    this.flyoutTop.set(relativeTop);
    this.activeFlyoutItem.set(item);

    // Focar no primeiro item do menu flutuante após renderização
    afterNextRender(
      () => {
        const flyout = document.querySelector('.nav-flyout');
        const firstLink = flyout?.querySelector<HTMLElement>('.nav-flyout__link');
        if (firstLink) {
          firstLink.focus();
        }
      },
      { injector: this.injector }
    );
  }

  /**
   * Rastrear mudanças de foco para índice de tabulação móvel
   */
  onMenuItemFocus(index: number): void {
    this.focusedItemIndex.set(index);
  }
}
