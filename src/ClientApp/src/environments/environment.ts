// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  tenantOptions:{
    apiEndpoint: 'https://host.docker.internal:44314',
    apiVersion: '2'
  },
  auth: {
    issuer: 'https://host.docker.internal:44316',
    redirectUri: 'https://localhost:4200/auth/login-completed',
    postLogoutRedirectUri: 'https://localhost:4200/auth/logout-completed',
    clientId: 'tenants_admin',
    responseType: 'code',
    scope: 'openid profile roles tenants-api',
    basePath : 'https://localhost:4200/auth',
  },
  layout:{
    brand: "cfio",
    defaultMenuOpen: true,
    userImageUrl: "https://i.pravatar.cc/300"//"https://host.docker.internal:44316/Account/Auth/ProfileImage?username={username}"
  }
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
