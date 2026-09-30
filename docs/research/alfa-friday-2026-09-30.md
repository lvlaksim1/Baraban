# Alfa-Пятница / Tasty Coffee — session reconstruction 2026-09-30

This note intentionally contains **no cookies, tokens, CSRF values or X-GIB values**.

## Captured page

- Alfa-Friday page: `https://link.alfabank.ru/partner-offers/friday/21898`
- Offer: Tasty Coffee
- Captured offer id: `21898`

## Verified request sequence

1. `POST /partner-offers/api/LoyaltyRouletteService/getAdvertCampaign`, body `{}`.
2. `POST /partner-offers/api/LoyaltyRouletteService/getCustomerOffersDrum`, captured body `{"advertCampaignId":21971}`.
3. `POST /partner-offers/api/LoyaltyRouletteService/getOfferDrums`, captured body `{"offerDrumId":[21965,21963,21964,21961,21969,21967]}`.
4. `POST /partner-offers/api/LoyaltyRouletteService/confirmDrumOffer`, captured body `{"advertCampaignId":21971,"offerWinId":21967}`.
5. After confirmation the site requested `getOfferDrums` again with `{"offerDrumId":[21967]}`.

The numeric IDs above are evidence from this one session, not protocol constants.

## Verified site logic

The loaded production JS shows:

- `getCustomerOffersDrum({ advertCampaignId })` returns `available` and `offerWinId`.
- `available` is passed directly to `getOfferDrums({ offerDrumId: available })`.
- The winner is found by comparing `offerDrumId` with `offerWinId`.
- Confirmation calls `confirmDrumOffer({ advertCampaignId, offerWinId })`.
- The Alfa-Friday offer model exposes `drumId`, which the site matches to the advertising campaign id.

The module therefore reads `/api/v1/offer/{offerId}` and captures `$.drumId` as `advertCampaignId` instead of hard-coding 21971.

## Request-specific security headers

The capture proves that `X-GIB-FGSSCw-alfa-le` and `X-GIB-GSSCw-alfa-le` change between drum calls. A single global copied security-header set is therefore not a reliable replay model.

Baraban now stores request-specific header profiles keyed by method + endpoint and can populate them either from live WebView2 traffic or by importing a recorder ZIP containing `requests.json`.

Actual sensitive values must never be committed to this public repository.
