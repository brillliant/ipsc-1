# ipsc-1 — VR-тир IPSC/USPSA на Meta Quest

Unity 6000.3, Meta XR SDK (OpenXR, MR/passthrough, MRUK), Interaction SDK. Пользователь тестирует на очках (APK), редактор Unity обычно открыт.
Отвечать по-русски, коротко, с таблицами для чисел; честно называть, что не проверено в очках.

## Структура
- `Assets/scripts/` — геймплей: `PistolScript`, `SlideScript`, `MagazineScript`, `HandScript`, `BulletScript` (пробоина + Destroy), `TargetScript` (реакция мишени: звук, падение поппера, reset), `Main` (ввод с контроллеров, старт/стоп), `FloorScript` (пол, калибровка), `HandednessService` (поза пистолета, левша).
- `Assets/scripts/menu/` — `BuilderService` (расстановка, зоны, загрузка/сохранение стейджей, лупа Inspect), `MenuController` (кнопки меню, стейты), `CompetitionModeService` (прогон: команды, таймер, hintText/readyText), `ReadyStagesMenu`.
- `Assets/scripts/enum/StateEnum.cs` — режимы: расстановка объектов, Remove, MoveStage, DrawShootingZone, Inspect, StageRun.
- `Assets/prefubs/` — модели (имя префаба = ключ в prefabMap и в JSON стейджей, с суффиксом `(Clone)`): USPSA_target, USPSA_no_shot_target, USPSA_target_left/right/left_right_half_dark, ipsc_target_prefub, ipsc_target_noShot_prefub, USPSAMiniPopperPrefub, USPSAPopperPrefub, barrel2prefub, wallPrefub (8 ft), wall4Prefub (4 ft). Для каждого есть *Preview-префаб.
- `Assets/Resources/PresetStages/<Категория>/*.json` — готовые стейджи, кнопка в меню появляется по имени файла. Папка = пункт дропдауна категорий в панели Ready stages (сейчас `Classifiers`, 20 шт.); новая категория = новая папка + активированный пункт дропдауна с тем же названием в сцене. Пункты `All` и `Custom` (стейджи игрока из persistentDataPath) зашиты. Префикс `DRAFT_` = черновик.
- `Assets/Scenes/GameScene.unity` — единственная сцена. Меню — инстанс семплового префаба Meta (см. ниже).

## Конвенции кода (пользователь на них настаивает)
- Звуки: `public AudioSource`-поля, назначаются в инспекторе, `PlayOneShot(clip)`. Не `GameObject.Find`, не `PlayClipAtPoint`, не `[HideInInspector]` на публичных полях.
- Новый объект для расстановки = поле Preview/Prefab в BuilderService + ссылка в сцене + запись в prefabMap + значение StateEnum + ветка в `BuilderService.Update` + choose-метод в MenuController + `isPlacementState` в CompetitionModeService + тайл меню.
- Стиль: K&R скобки, русские имена там, где уже есть (`установленныеМишени`, `пробоины`), методы lowerCamelCase.
- Делать «по аналогии с тем, как уже сделано»; настройки предпочитать коду («код — крайняя мера»); откатывать сразу, если пользователь просит.
- Код не компилируется локально: после правок просить Refresh (Ctrl+R) в Unity, при правке сцены — переоткрыть её без сохранения. Редактор может перезаписать правки с диска своей копией.

## Меню (семпловый префаб Meta)
- Инстанс `ContentUIExample-HorizonOS3` (PrefabInstance 1471377434), сам префаб в PackageCache, в Assets его нет. Кнопки = overrides вложенных тайлов. Тайл = Toggle; onValueChanged дёргается и при включении, и при выключении.
- Подсветка нажатого и группа сделаны конфигом в сцене: Toggle.m_Group → ToggleGroup на Content (added component), AnimatorOverrideLayerWeigth добавлен каждому тайлу (в варианте Regular он удалён), аниматор — `Assets/animation/TileToggle_Dark.controller` через тему `Assets/animation/UITheme_Tiles_Dark.asset` (UIThemeManager иначе перекрывает контроллер). У Toggle в префабе isOn=1 по умолчанию — override m_IsOn 0 обязателен.
- Колонка действий справа: LayoutElement ignoreLayout + ручной RectTransform. Запасных тайлов в сетке два.
- Подробности и id — в памяти агента (`menu-meta-sample-prefab`).

## Стейджи и классифаеры
- JSON: `objectDataList` (prefabName, position, rotation — локально к stageRoot), `shootingZones` (полилинии), `startMarks` (две точки под ноги). Необязательный блок `info` (code, procedure, scoring, briefing, image — путь в Resources без расширения) — панель Stage info справа от списка стейджей; у пользовательских стейджей его нет, панель скрыта. Картинки классифаеров: `Resources/PresetStages/Classifiers/img/<Имя>.png` (Sprite, из PDF-брифингов uspsa.org; скрипт сборки info — scratchpad build_info.py). Вниз по стрельбищу −Z, влево от стрелка +X, мишени y=0.002, стены y=0.811.
- Схемы: реальные PDF `https://uspsa.org/viewer/pdf/<код>.pdf` (с Referer `uspsa.org/viewer/<код>.pdf`); рендер страниц PyMuPDF (`import fitz`). На новых Build Diagram сетка 1 клетка = 1 фут — считать клетки, не пиксели.
- Зоны стрелка могут быть не соединены — перебегать вне зоны нормально по правилам USPSA.
- Нет моделей: диагональный хардкавер, мишень с чёрным низом, перевёрнутый/низкий no-shoot, стол/стул, низкая стена (пользователь отказался).

## Режимы и кнопки
- Левый контроллер: стик-клик — меню; Y — стереть пробоины (и магазины с пола) / выход из Inspect и Remove; grip — старт/стоп прогона; стик вверх — сброс прогона (и после «Your time»).
- Inspect («лупа»): курок по бумажной мишени — копия с пробоинами на земле, 0.8 м, 45° влево, с ореолом; курок по копии убирает её.
- Контур guardian под OpenXR приложению недоступен — не предлагать.

## Отладка
- Логи с очков: `adb logcat -s Unity` (adb в `C:/Program Files/Unity/Hub/Editor/6000.3.6f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/`), пакет `com.xrdevelopment.practicalshooting`. Буфер увеличивать: `adb logcat -G 16M`, иначе XR-шум вытесняет строки.
- Правка YAML сцены/префабов напрямую — рабочий приём; guid в .meta сохранять; сцена может быть в CRLF или LF — сохранять как есть.
