import { HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private auth: OAuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    // Get the auth token from the service.
    if(this.auth.hasValidAccessToken()){
        const authToken = this.auth.getAccessToken();

        // Clone the request and replace the original headers with
        // cloned headers, updated with the authorization.
        const authReq = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${authToken}`)
        });

        // send cloned request with header to the next handler.
        return next.handle(authReq);
    }
    return next.handle(req);
  }
}