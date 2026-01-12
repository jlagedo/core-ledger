import {ChangeDetectionStrategy, Component, input} from '@angular/core';
import {BreadcrumbComponent} from '../breadcrumb/breadcrumb';

@Component({
  selector: 'app-page-header',
  imports: [BreadcrumbComponent],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  title = input.required<string>();
  additionalClasses = input<string>('');
}
