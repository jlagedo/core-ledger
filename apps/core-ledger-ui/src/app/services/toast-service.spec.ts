import { TestBed } from '@angular/core/testing';

import { ToastService } from './toast-service';

// ToastService is tested indirectly through LoggerService tests
// and through components that use it. No need for isolated unit tests
// since it's a simple state management service.
