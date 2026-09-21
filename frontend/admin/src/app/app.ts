import { Component } from '@angular/core';
import { AdminLayout } from './layout/admin-layout';

@Component({
  imports: [AdminLayout],
  selector: 'app-root',
  template: '<app-admin-layout />',
})
export class App {}
