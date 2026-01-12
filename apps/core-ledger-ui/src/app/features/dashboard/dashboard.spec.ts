import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { Dashboard } from './dashboard';
import { provideTestDependencies } from '../../testing/test-helpers';

// Mock ResizeObserver for ngx-echarts
class ResizeObserverMock {
  observe() { }
  unobserve() { }
  disconnect() { }
}

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    // Add ResizeObserver polyfill
    globalThis.ResizeObserver = ResizeObserverMock as any;

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [...provideTestDependencies(), provideHttpClientTesting()]
    })
      .compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
