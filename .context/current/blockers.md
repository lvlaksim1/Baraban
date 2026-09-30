# Current blockers and open risks

- The retained Postman collection contains the confirmed `confirmDrumOffer` requests but not the exact historical request contracts for `getCustomerOffersDrum` and `getOfferDrums`.
- Therefore the first module is compilable but not yet claimed to be end-to-end live verified.
- Fresh authenticated traffic is required to validate those two request bodies, response shapes and the source of the current `advertCampaignId`.
