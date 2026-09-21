import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ContainerComponent } from '@coreui/angular';

/** Estructura común de la aplicación administrativa: barra superior y área de contenido. */
@Component({
  selector: 'app-admin-layout',
  imports: [ContainerComponent, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.html',
})
export class AdminLayout {}
