import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login'; // ✅ link the component
import { RegisterComponent } from './pages/register/register';
import { authGuard } from './guards/auth-guard';
import { LayoutComponent } from './layout/layout/layout';
import { HomeComponent } from './pages/home/home';
import { AboutComponent } from './pages/about/about';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: '', component: LayoutComponent, canActivate: [authGuard], children: [
    // pages will be added here as we build them
  ] },
];