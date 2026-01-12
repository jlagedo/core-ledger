import {ComponentFixture, TestBed} from '@angular/core/testing';
import {BreadcrumbComponent} from './breadcrumb';
import {provideTestDependencies} from '../../testing/test-helpers';

describe('BreadcrumbComponent', () => {
  let component: BreadcrumbComponent;
  let fixture: ComponentFixture<BreadcrumbComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BreadcrumbComponent],
      providers: provideTestDependencies(),
    })
      .compileComponents();

    fixture = TestBed.createComponent(BreadcrumbComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
