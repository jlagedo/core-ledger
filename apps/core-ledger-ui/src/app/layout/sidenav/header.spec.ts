import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Sidenav } from './sidenav';
import { provideTestDependencies } from '../../testing/test-helpers';

describe('Sidenav', () => {
  let component: Sidenav;
  let fixture: ComponentFixture<Sidenav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Sidenav],
      providers: [...provideTestDependencies(), provideHttpClientTesting()],
    })
      .compileComponents();

    fixture = TestBed.createComponent(Sidenav);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
