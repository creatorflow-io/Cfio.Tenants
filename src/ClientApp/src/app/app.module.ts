import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { environment } from '../environments/environment';
import { TenantsModule } from '@juice-js/tenants';
import { TranslateModule } from '@ngx-translate/core';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material.module';
import { OAuthModule } from 'angular-oauth2-oidc';
import { LayoutModule } from '@juice-js/layout';
import { AuthModule } from '@juice-js/auth';
import { AuthInterceptor } from './auth.interceptor';

const { tenantOptions, auth, layout, production }  = environment;

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    LayoutModule.forRoot(production, layout),
    TenantsModule.forRoot(tenantOptions),
    TranslateModule.forRoot(),
    AuthModule.forRoot(auth),
    OAuthModule.forRoot(),
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MaterialModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
