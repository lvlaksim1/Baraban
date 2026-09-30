# Manager beliefs

- Baraban is a multi-drum Windows application, not an Alfa-only client.
  - source: owner directive, 2026-09-30
  - authority: owner-directive
- Browser login and manual cookies/headers import are both required authentication paths.
  - source: owner directive, 2026-09-30
  - authority: owner-directive
- Cookies must persist for as long as the server accepts them; Baraban must not impose a shorter lifetime.
  - source: owner directive, 2026-09-30
  - authority: owner-directive
- HTTP parameters must be inspectable and editable by the user.
  - source: owner directive, 2026-09-30
  - authority: owner-directive
- The .NET 8 WPF/WebView2 MVP builds successfully on GitHub Actions Windows runner.
  - source: workflow run 36650042799 for commit 84f1750bbbeb21e0240d5d57d0a38498370efd84
  - authority: verified-ci
- The historical Postman capture verifies `POST /partner-offers/api/LoyaltyRouletteService/confirmDrumOffer`; exact bodies for the two read requests are not present in the retained collection.
  - source: retained Postman collections from 2026-09-17
  - authority: verified-repository
