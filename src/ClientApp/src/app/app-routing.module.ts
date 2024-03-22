import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@juice-js/auth';
import { PageComponent, PageNotfoundComponent } from '@juice-js/layout';
import { TenantsComponent } from '@juice-js/tenants';


const routes: Routes = [
  {
    path:'',
    component: PageComponent,
    data:{
      menuDisplay: true,
      menuIcon: 'admin_panel_settings',
      menuTitle: 'System',
      menuOrder: 1,
      settingUrl: ':tenant/settings'
    },
    children:[
      {
        path:'tenants',
        data:{
          menuDisplay: true,
          menuIcon: 'business',
          menuTitle: 'Tenants',
        },
        canActivate: [AuthGuard],
        component: TenantsComponent
      },
      {
        path:'auth',
        data:{
          menuDisplay: true
        },
        loadChildren: () => import('@juice-js/auth').then(m =>{
          return m.AuthRoutingModule;
        })
      },
      {
        path:'**',
        component: PageNotfoundComponent
      }
    ]
  }
];


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }