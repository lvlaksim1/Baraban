# Baraban

Windows-приложение для модульных сценариев «барабанов призов» и похожих HTTP-workflow.

## Текущее состояние

В приложении два Alfa-модуля:

- **Alfa-Пятница — Tasty Coffee (30.09.2026)** — текущий рабочий модуль, восстановленный по браузерной сессии.
- **[АРХИВ] Alfa — Loyalty Roulette (снимок 17.09.2026)** — сохранён без дальнейшего развития; автоматический запуск цепочки для архивных модулей отключён.

## Что умеет приложение

- Несколько независимых барабанов через `Baraban/Drums/*.json`.
- Обычный вход через встроенный WebView2 с постоянным профилем.
- Альтернативная работа с сохранённой сессией:
  - ручной `Cookie:` header;
  - полный редактируемый Session JSON;
  - импорт ZIP браузерного рекордера, если внутри есть `requests.json`.
- Локальное хранение сессии в `%LOCALAPPDATA%\Baraban\session.bin` с защитой Windows DPAPI.
- Cookies не получают искусственный TTL от Baraban: серверный expiry/rejection остаётся авторитетным.
- Request-specific профили заголовков. Это важно для защитных `X-GIB-*`, которые в реальной сессии различались между endpoint'ами.
- Редактирование Method / URL / query / headers / body / Cookie override.
- Последовательные запросы с `{{variable}}` и JSON-capture.
- Таблица вариантов призов и выделение `★ WINNER`.
- Подтверждающие запросы не выполняются автоматически.

## Рабочий Alfa-Пятница workflow

Файл: `Baraban/Drums/alfa-friday-tasty-coffee-2026-09-30.json`.

Текущий offer URL:

```
https://link.alfabank.ru/partner-offers/friday/21898
```

Цепочка:

1. `GET /partner-offers/api/v1/offer/{offerId}`
   - `$.drumId -> advertCampaignId`
2. `POST /partner-offers/api/LoyaltyRouletteService/getCustomerOffersDrum`
   - body: `{"advertCampaignId": {{advertCampaignId}}}`
   - `$.available -> available`
   - `$.offerWinId -> offerWinId`
3. `POST /partner-offers/api/LoyaltyRouletteService/getOfferDrums`
   - body: `{"offerDrumId": {{available}}}`
4. Baraban показывает варианты и отмечает строку, где `offerDrumId == offerWinId`.
5. `confirmDrumOffer` доступен только как отдельный явный запрос.

Для диагностики в модуле сохранён и реальный `getAdvertCampaign({})`, но рабочая цепочка не нуждается в жёстко прошитом `advertCampaignId`.

Санитизированное восстановление исходной сессии: `docs/research/alfa-friday-2026-09-30.md`.

## Почему request-specific headers

В записи 30.09.2026 значения защитных заголовков различались между:

- `getCustomerOffersDrum`;
- `getOfferDrums`;
- `confirmDrumOffer`.

Поэтому Baraban хранит заголовки по ключу `METHOD + endpoint`, а не только один глобальный набор.

При импорте recorder ZIP реальные cookies/CSRF/X-GIB остаются локально и записываются только в DPAPI-защищённую сессию. В публичный репозиторий они не попадают.

## Сборка

Требования:

- Windows 10/11;
- .NET 8 SDK;
- Microsoft Edge WebView2 Runtime.

```powershell
dotnet restore Baraban/Baraban.csproj
dotnet build Baraban/Baraban.csproj -c Release
```

GitHub Actions собирает проект на Windows.

## Безопасность репозитория

Репозиторий публичный. Реальные cookies, токены, CSRF, `X-GIB-*` и другие значения пользовательской сессии запрещено коммитить.
