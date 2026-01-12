import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {BreadCrumbService} from '../../services/bread-crumb-service';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-breadcrumb',
  imports: [
    RouterLink
  ],
  templateUrl: './breadcrumb.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BreadcrumbComponent {
  breadcrumbService = inject(BreadCrumbService);
}
