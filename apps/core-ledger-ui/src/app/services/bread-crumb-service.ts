import {inject, Injectable} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {toSignal} from '@angular/core/rxjs-interop';
import {defer, filter, map, startWith} from 'rxjs';

export interface Breadcrumb {
  label: string;
  url: string;
}

@Injectable({
  providedIn: 'root',
})
export class BreadCrumbService {
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  breadcrumbs = toSignal(
    defer(() =>
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd),
        startWith(null), // Emit immediately to capture initial route state
        map(() => {
          const breadcrumbs = this.createBreadcrumbs(this.activatedRoute.root);
          return this.deduplicateBreadcrumbs(breadcrumbs);
        })
      )
    ),
    {initialValue: []}
  );

  private createBreadcrumbs(
    route: ActivatedRoute,
    url: string = '',
    breadcrumbs: Breadcrumb[] = []
  ): Breadcrumb[] {
    const children: ActivatedRoute[] = route.children;

    if (children.length === 0) {
      return breadcrumbs;
    }

    for (const child of children) {
      let routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');

      // For routes with loadChildren but no component, the URL segment might be
      // in routeConfig.path. Use it if snapshot.url is empty but path is defined.
      if (routeURL === '' && child.routeConfig?.path) {
        const configPath = child.routeConfig.path;
        // Handle parameterized paths by substituting actual param values
        if (configPath.includes(':')) {
          routeURL = configPath
            .split('/')
            .map(segment => {
              if (segment.startsWith(':')) {
                const paramName = segment.slice(1);
                return child.snapshot.params[paramName] ?? segment;
              }
              return segment;
            })
            .join('/');
        } else {
          routeURL = configPath;
        }
      }

      if (routeURL !== '') {
        url += `/${routeURL}`;
      }

      const label = child.snapshot.data['breadcrumb'];
      if (label) {
        breadcrumbs.push({label, url});
      }

      return this.createBreadcrumbs(child, url, breadcrumbs);
    }

    return breadcrumbs;
  }

  private deduplicateBreadcrumbs(breadcrumbs: Breadcrumb[]): Breadcrumb[] {
    const seen = new Set<string>();
    return breadcrumbs.filter(breadcrumb => {
      if (seen.has(breadcrumb.url)) {
        return false;
      }
      seen.add(breadcrumb.url);
      return true;
    });
  }
}
