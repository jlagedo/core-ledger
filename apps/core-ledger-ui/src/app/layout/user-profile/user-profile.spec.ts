import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { UserProfile } from './user-profile';
import { ThemeService } from '../../services/theme-service';
import { provideTestDependencies } from '../../testing/test-helpers';

describe('UserProfile', () => {
  let component: UserProfile;
  let fixture: ComponentFixture<UserProfile>;
  let themeService: ThemeService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserProfile],
      providers: provideTestDependencies(),
    }).compileComponents();

    fixture = TestBed.createComponent(UserProfile);
    component = fixture.componentInstance;
    themeService = TestBed.inject(ThemeService);

    // Set required input
    fixture.componentRef.setInput('isCollapsed', false);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('theme toggle functionality', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('isCollapsed', false);
      fixture.detectChanges();
    });

    it('should toggle theme when theme button is clicked', () => {
      themeService.setTheme('dark');
      fixture.detectChanges();

      const toggleSpy = vi.spyOn(themeService, 'toggleTheme');
      const themeButton = fixture.nativeElement.querySelector('[aria-label*="Switch to"]');

      themeButton.click();
      expect(toggleSpy).toHaveBeenCalled();
    });
  });
});
