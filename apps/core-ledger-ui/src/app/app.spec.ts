import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { App } from './app';
import { provideTestDependencies } from './testing/test-helpers';

// App component is a simple container with sidenav and router-outlet.
// Its behavior is tested through integration and E2E tests.
// Component structure tests were removed per TESTING_GUIDELINES.md
