import { Component, DebugElement } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { SortableDirective, SortEvent } from './sortable.directive';

@Component({
  template: `
    <table>
      <thead>
        <tr>
          <th sortable="name" (sort)="onSort($event)">Name</th>
          <th sortable="age" (sort)="onSort($event)">Age</th>
        </tr>
      </thead>
    </table>
  `,
  imports: [SortableDirective]
})
class TestComponent {
  lastSortEvent?: SortEvent;

  onSort(event: SortEvent) {
    this.lastSortEvent = event;
  }
}

describe('SortableDirective', () => {
  let component: TestComponent;
  let fixture: ComponentFixture<TestComponent>;
  let nameHeader: DebugElement;
  let ageHeader: DebugElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestComponent, SortableDirective]
    }).compileComponents();

    fixture = TestBed.createComponent(TestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const headers = fixture.debugElement.queryAll(By.directive(SortableDirective));
    nameHeader = headers[0];
    ageHeader = headers[1];
  });

  it('should cycle through sort directions on click', () => {
    const nameElement = nameHeader.nativeElement as HTMLElement;

    // First click: '' -> 'asc'
    nameElement.click();
    fixture.detectChanges();
    expect(component.lastSortEvent).toEqual({ column: 'name', direction: 'asc' });

    // Second click: 'asc' -> 'desc'
    nameElement.click();
    fixture.detectChanges();
    expect(component.lastSortEvent).toEqual({ column: 'name', direction: 'desc' });

    // Third click: 'desc' -> ''
    nameElement.click();
    fixture.detectChanges();
    expect(component.lastSortEvent).toEqual({ column: 'name', direction: '' });

    // Fourth click: '' -> 'asc' (cycle repeats)
    nameElement.click();
    fixture.detectChanges();
    expect(component.lastSortEvent).toEqual({ column: 'name', direction: 'asc' });
  });

  it('should emit sort event with correct column name', () => {
    const ageElement = ageHeader.nativeElement as HTMLElement;

    ageElement.click();
    fixture.detectChanges();

    expect(component.lastSortEvent).toEqual({ column: 'age', direction: 'asc' });
  });
});
