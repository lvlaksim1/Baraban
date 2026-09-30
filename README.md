# Baraban

Windows-приложение для модульных сценариев «барабанов призов» и других похожих HTTP-workflow.

## Что уже есть

- **Несколько барабанов**: определения загружаются из `Baraban/Drums/*.json`. Ядро приложения не привязано к одному API.
- **Обычная авторизация через сайт** во встроенном WebView2 с постоянным профилем.
- **Долговечная сессия**:
  - профиль WebView2 хранится в `%LOCALAPPDATA%\Baraban\WebView2`;
  - HTTP-копия cookies/headers хранится в `%LOCALAPPDATA%\Baraban\session.bin`;
  - `session.bin` защищён Windows DPAPI и читается только текущим Windows-пользователем;
  - приложение не вводит собственный короткий TTL: cookie используется, пока не истёк её серверный expiry или сервер не перестал её принимать.
- **Ручная авторизация**:
  - импорт обычного `Cookie:` header;
  - полный Session JSON с cookies и headers можно просматривать и редактировать.
- **HTTP inspector/editor**:
  - method;
  - URL (включая query string);
  - headers;
  - Cookie override;
  - body;
  - response.
- **Редактирование запросов сохраняется** обратно в JSON-модуль барабана.
- Перед отправкой запроса/цепочки приложение синхронизирует актуальные cookies из встроенного браузера.
- `confirmDrumOffer` помечен как подтверждающее действие и **не запускается автоматически**.

## Первый модуль: Alfa — Loyalty Roulette

Файл: `Baraban/Drums/alfa-loyalty-roulette.json`.

Подтверждённый по ранее записанной сетевой активности endpoint:

```
POST https://link.alfabank.ru/partner-offers/api/LoyaltyRouletteService/confirmDrumOffer
```

Исторический пример payload из сетевого захвата:

```json
{
  "advertCampaignId": 21541,
  "offerWinId": 21445
}
```

Эти ID **не зашиты как рабочие значения**. Они приведены только как пример структуры.

Модуль также содержит заготовки для:

- `getCustomerOffersDrum`;
- `getOfferDrums`;
- извлечения `offerWinId`;
- построения таблицы призов;
- сопоставления `offerDrumId == offerWinId`;
- отображения `★ WINNER`.

Важно: точные тела `getCustomerOffersDrum` и `getOfferDrums` не были сохранены в доступной Postman-коллекции, поэтому текущие body в модуле отмечены как редактируемые шаблоны. Перед живым использованием их нужно сверить с актуальной сетевой активностью браузера. `confirmDrumOffer` восстановлен из реального захвата.

## Архитектура

```
WPF shell
├── WebView2 login/profile
├── SessionStore
│   ├── persistent cookies
│   ├── editable headers
│   └── DPAPI encrypted local storage
├── HTTP editor/executor
├── DrumRepository
│   └── Drums/*.json
├── WorkflowRunner
│   ├── sequential requests
│   ├── JSON captures → variables
│   └── {{variable}} substitution
└── ResultProjector
    └── prize table / winner highlighting
```

Слой авторизации отделён от определения барабана. Один и тот же Session может использоваться несколькими модулями.

## Сборка

Требования:

- Windows 10/11;
- .NET 8 SDK;
- Microsoft Edge WebView2 Runtime.

```powershell
dotnet restore Baraban/Baraban.csproj
dotnet build Baraban/Baraban.csproj -c Release
```

CI собирает проект на Windows runner при изменениях кода.

## Безопасность репозитория

Репозиторий публичный. Реальные cookies, CSRF-токены, `X-GIB-*` и другие значения пользовательской сессии в исходники **не добавляются**.

