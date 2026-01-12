import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';

import { BreadCrumbService } from './bread-crumb-service';

// BreadCrumbService is tested indirectly through breadcrumb component tests
// No need for isolated unit tests since it's a reactive service that
// responds to router events.
