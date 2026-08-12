# ASH:BORN 시스템 아키텍처

이 문서는 `Assets/Scripts`의 실제 클래스와 책임을 기준으로 정리한 Mermaid 아키텍처 문서입니다. 저장소의 씬과 상용 리소스는 공개 범위에서 제외되어 있으므로, 코드에서 확인 가능한 런타임 흐름과 책임 경계를 중심으로 설명합니다.

## 1. 전체 클라이언트 흐름

```mermaid
flowchart TD
    subgraph Persistent["전역 수명주기 · 영구 시스템"]
        GM["GameManager<br/>게임 상태 · 씬 전환 · 결과 정산"]
        SCENE["SceneLoader<br/>Fade · Scene Load"]
        PDM["PlayerDataManager<br/>JSON 저장 · 세션 복원"]
        WALLET["PlayerWallet<br/>보유 골드 · 세션 골드 정산"]
        SET["SettingManager<br/>오디오 · FPS · 조이스틱 설정"]
        ADS["AdSDK / AdFlowController<br/>광고 상태 · 완료 콜백"]
        POOL["ObjectPoolManager<br/>적·투사체·이펙트 재사용"]
    end

    subgraph Lobby["LobbyScene"]
        LUI["LobbyUIManager<br/>던전 진입 · 강화 · 설정"]
        SHOP["ShopController<br/>구매 · 판매 · 재고 검증"]
        UPGRADE["Stat Upgrade UI<br/>영구 스탯 강화"]
    end

    subgraph InGame["InGameScene"]
        IGM["InGameManager<br/>타이머 · 사망 · 탈출 판정"]
        PLAYER["Player Components<br/>Movement · Health · Stats · Progress"]
        MAP["MapManager<br/>타일 조합 · 구역 등급 · NavMesh"]
        SPAWNER["EnemySpawner<br/>난이도별 적 배치 · 프레임 분산"]
        ENEMY["Enemy Runtime<br/>Detection · Behavior · Attack Strategy"]
        CULL["EnemyCullingManager<br/>거리 기반 활성화 · 분할 검사"]
        SKILL["SkillManager<br/>ISkill 생성 · 레벨 · 마나"]
        ITEM["Inventory / Equipment / QuickSlot"]
        UI["UIManager / InGameHUDController<br/>패널 · HUD · 일시정지"]
    end

    LUI -->|"GoToGame"| GM
    GM -->|"Scene load 요청"| SCENE
    SCENE -->|"InGameScene"| IGM

    IGM --> PLAYER
    IGM --> MAP
    MAP -->|"타일 생성 · NavMesh 완료"| SPAWNER
    SPAWNER -->|"Spawn(poolId)"| POOL
    POOL --> ENEMY
    CULL -->|"거리 기준 활성/비활성"| ENEMY
    PLAYER --> CULL
    PLAYER <-->|"전투"| ENEMY
    PLAYER --> SKILL
    PLAYER --> ITEM
    IGM --> UI

    SHOP --> ITEM
    SHOP --> WALLET
    UPGRADE --> PLAYER
    PDM <-->|"인벤토리 · 장비 · 퀵슬롯"| ITEM
    PDM <-->|"골드"| WALLET
    PDM <-->|"영구 스탯"| PLAYER

    GM -->|"사망 / 탈출"| UI
    GM -->|"EndStage"| WALLET
    GM -->|"스테이지 종료"| ADS
    GM -->|"풀 객체 회수"| POOL
    ADS -->|"완료 콜백"| GM
    GM -->|"LobbyScene 요청"| SCENE
    SCENE -->|"LobbyScene"| LUI
    SET --> UI
```

### 책임 경계

| 영역 | 핵심 책임 | 대표 코드 |
|---|---|---|
| 전역 게임 흐름 | 상태 전환, 씬 이동, 게임 종료 정산 | `GameManager`, `SceneLoader` |
| 인게임 세션 | 타이머, 사망, 탈출 성공 여부 | `InGameManager`, `EscapePortal3D` |
| 맵 생성 | 타일 조합, 구역 등급, 이벤트 타일, NavMesh 생성 | `MapManager`, `Tile` |
| 적 생성과 재사용 | 난이도별 적 선택, 풀링 객체 배치, 재사용 초기화 | `EnemySpawner`, `ObjectPoolManager`, `EnemyController` |
| 런타임 부하 제어 | 거리 기반 적 활성화, 분할 검사 | `EnemyCullingManager` |
| 플레이어 런타임 | 이동, 체력, 경험치, 스탯 | `PlayerMovement`, `PlayerHealth`, `PlayerProgress`, `PlayerRuntimeStats` |
| UI | HUD와 패널 표시, 일시정지 | `UIManager`, `InGameHUDController`, `LobbyUIManager` |
| 데이터 지속성 | JSON 저장과 씬 간 세션 복원 | `PlayerDataManager` |

## 2. 인벤토리 · 장비 · 퀵슬롯

```mermaid
flowchart TD
    subgraph StaticData["정적 데이터 · ScriptableObject"]
        LIST["ItemDataListSO<br/>아이템 카탈로그"]
        ITEMSO["ItemDataSO<br/>ID · 타입 · 가격 · 스탯 · 스택"]
        DROP["ItemDropTableSO<br/>가중치 · 드롭 개수"]
        LIST --> ITEMSO
        DROP --> ITEMSO
    end

    subgraph RuntimeModel["런타임 아이템 모델"]
        INSTANCE["ItemInstance<br/>Guid · ItemDataSO · quantity"]
        SLOTDATA["InventorySlotData[]"]
        INV["InventoryManager<br/>스택 · 교환 · 전송 · 제거 · 이벤트"]
        ITEMSO --> INSTANCE
        INSTANCE --> SLOTDATA
        SLOTDATA --> INV
    end

    subgraph SlotUI["슬롯 UI 역할 분리"]
        BASE["ItemSlot<br/>공통 표시 · 일반 이동/드롭"]
        EQUIP_SLOT["EquipSlot<br/>장비 타입 검증 · 장착/해제"]
        QUICK_SLOT["QuickSlot<br/>아이템 소유 없이 Guid 참조"]
        BASE --> EQUIP_SLOT
        BASE --> QUICK_SLOT
    end

    subgraph DomainManagers["도메인별 관리자"]
        EQUIP["EquipmentManager<br/>장비 소유 · 스탯 적용/제거"]
        QUICK["QuickSlotManager<br/>Guid 바인딩 · 이동/삭제 추적"]
        SHOP["ShopController<br/>공간 · 재고 · 골드 검증"]
        CHEST["ChestInteractable<br/>상자 인벤토리 열기/닫기"]
        USE["ItemUseSystem<br/>아이템 타입별 사용 진입점"]
    end

    INV -->|"OnInventoryChanged"| BASE
    INV -->|"OnItemMoved / OnItemRemoved"| QUICK
    EQUIP --> EQUIP_SLOT
    QUICK --> QUICK_SLOT
    INV --> EQUIP
    INV --> SHOP
    CHEST --> INV
    QUICK --> USE

    subgraph PlayerEffect["플레이어 반영"]
        EQUIP_HANDLER["EquipItemHandler"]
        CONSUME_HANDLER["ConsumeItemHandler"]
        CONTEXT["PlayerContext<br/>Health · RuntimeStats · Inventory"]
        EQUIP --> EQUIP_HANDLER
        USE --> CONSUME_HANDLER
        EQUIP_HANDLER --> CONTEXT
        CONSUME_HANDLER --> CONTEXT
    end

    subgraph Persistence["저장 · 복원"]
        PDM["PlayerDataManager"]
        JSON[("player_data.json")]
        INV -->|"ItemId · Amount · SlotIndex"| PDM
        EQUIP -->|"SlotType · ItemId"| PDM
        QUICK -->|"QuickSlotIndex · InventoryIndex"| PDM
        PDM --> JSON
        JSON --> PDM
        PDM -->|"Load / LoadSessionData"| INV
        PDM --> EQUIP
        PDM --> QUICK
    end
```

### 핵심 설계 포인트

- `ItemDataSO`는 변하지 않는 원형 데이터이고, 실제 획득 아이템은 `ItemInstance`입니다.
- 각 `ItemInstance`는 `Guid`를 가지므로 퀵슬롯은 인벤토리 슬롯 위치가 바뀌어도 같은 아이템을 추적할 수 있습니다.
- `ItemSlot`은 공통 UI와 일반 인벤토리 이동을 담당하고, `EquipSlot`과 `QuickSlot`은 서로 다른 소유권과 드롭 규칙을 확장합니다.
- 장비 슬롯은 아이템을 장비 도메인으로 이동시키고 스탯을 반영하지만, 퀵슬롯은 아이템을 복제하거나 소유하지 않고 GUID만 참조합니다.
- 인벤토리의 이동·삭제 이벤트를 `QuickSlotManager`가 구독하여 참조 무결성을 유지합니다.

## 3. 스킬 · 레벨업 성장

```mermaid
flowchart TD
    subgraph SkillData["스킬 데이터"]
        LIST["SkillDataListSO<br/>런타임 후보 목록"]
        DATA["SkillDataSO<br/>메타데이터 · 레벨 데이터 · 구현 타입"]
        LIST --> DATA
    end

    subgraph Progression["경험치 · 선택 흐름"]
        PROGRESS["PlayerProgress<br/>CurrentExp · CurrentLevel"]
        LEVEL["PlayerLevelManager<br/>필요 경험치 · 후보 필터링"]
        CARD["SkillCard[]<br/>무작위 후보 표시"]
        UIM["UIManager<br/>선택 중 일시정지 / 선택 후 재개"]
        PROGRESS -->|"AddExp / TryLevelUp"| LEVEL
        LEVEL --> CARD
        LEVEL --> UIM
    end

    subgraph Runtime["런타임 스킬 관리"]
        MANAGER["SkillManager<br/>등록 · 활성 상태 · 마나 · UpdateSkill"]
        TYPEMAP["ImplementationType → System.Type"]
        CONTRACT["ISkill<br/>Initialize · Activate · LevelUp · Cleanup"]
        BASE["BaseSkill<br/>공통 쿨다운 · 레벨 상태"]
        ATTACK["Attack Skills<br/>CrossSlash · SpiritFlame · HolyArea …"]
        BUFF["Buff Skills<br/>AttackBoost · DefenseBoost · Regen …"]
        UTILITY["Utility Skills<br/>GoldBonus · ExpBonus"]

        MANAGER --> TYPEMAP
        TYPEMAP -->|"AddComponent(type)"| CONTRACT
        CONTRACT --> BASE
        BASE --> ATTACK
        BASE --> BUFF
        BASE --> UTILITY
    end

    DATA --> MANAGER
    CARD -->|"선택한 SkillDataSO"| MANAGER
    MANAGER -->|"기존 스킬"| EXISTING["LevelUp(nextLevel)"]
    MANAGER -->|"신규 스킬"| NEW["Create → Initialize → LevelUp(1)"]
    EXISTING --> CONTRACT
    NEW --> CONTRACT

    subgraph PlayerRuntime["플레이어 런타임 반영"]
        STATS["PlayerRuntimeStats<br/>런타임 보정치"]
        HEALTH["PlayerHealth<br/>회복 · 최대 체력"]
        REWARD["RewardManager<br/>골드 · 경험치 보너스"]
        ATTACK --> STATS
        BUFF --> STATS
        BUFF --> HEALTH
        UTILITY --> REWARD
    end
```

### 핵심 설계 포인트

- `SkillDataSO`는 스킬 메타데이터와 레벨 데이터를 보유하고, 실제 실행은 `ISkill` 구현체가 담당합니다.
- `SkillManager`는 구현 타입을 `System.Type`으로 매핑하고 런타임에 컴포넌트를 생성합니다.
- 이미 보유한 스킬을 다시 선택하면 새 객체를 만들지 않고 기존 인스턴스의 레벨을 올립니다.
- `PlayerLevelManager`는 최대 레벨 스킬을 제외하고 후보를 구성하며, 선택 중에는 `UIManager`가 게임을 일시정지합니다.

## 4. 생성·활성화 최적화 흐름

```mermaid
flowchart LR
    subgraph Spawn["생성 및 재사용"]
        REQUEST["EnemySpawner<br/>Spawn 요청"] --> POOL{"ObjectPoolManager"}
        POOL -- "대기 객체 있음" --> REUSE["기존 객체 재사용"]
        POOL -- "대기 객체 없음" --> CREATE["새 객체 생성"]
        CREATE --> RESET["OnSpawnInitialize"]
        REUSE --> RESET
        RESET --> ACTIVE["활성 적"]
    end

    subgraph Runtime["런타임 갱신 범위 제어"]
        PLAYER["Player 이동"] --> THRESHOLD{"이동 임계값 초과?"}
        THRESHOLD -- No --> SKIP["검사 생략"]
        THRESHOLD -- Yes --> BATCH["checksPerFrame만 검사"]
        BATCH --> DIST["sqrMagnitude 거리 비교"]
        DIST --> NEAR["근거리: 활성"]
        DIST --> FAR["원거리: 비활성"]
        NEAR --> ACTIVE
    end

    subgraph Return["반환"]
        ACTIVE --> END{"사망·사용 종료·스테이지 종료"}
        END --> DESPAWN["SetActive(false)"]
        DESPAWN --> POOL
    end
```

### 핵심 설계 포인트

- 반복 생성되는 적과 전투 객체는 `ObjectPoolManager`를 통해 재사용합니다.
- 풀에서 꺼낸 적은 `EnemyController.OnSpawnInitialize`를 거쳐 이전 생명주기의 상태를 초기화합니다.
- `EnemyCullingManager`는 플레이어가 일정 거리 이상 움직인 경우에만 검사를 수행합니다.
- 검사 대상도 한 프레임에 전부 처리하지 않고 `checksPerFrame` 단위로 나눕니다.
- 거리 비교에는 `sqrMagnitude`를 사용하여 제곱근 계산을 피합니다.

> 현재 공개 코드에서는 이동 판정과 분할 검사 진행이 결합되어 있어, 첫 chunk 이후 플레이어가 멈추면 남은 대상 검사가 이어지지 않을 수 있습니다. 전체 scan 완료를 보장하는 상태 기반 구현은 아직 반영되지 않았으며, 자세한 진단은 [기술 문서](../technical/README.md#1-enemy-multi-frame-scan-stopping-after-the-first-chunk)에 기록합니다.

## 5. 런타임 맵 조합 흐름

```mermaid
flowchart TD
    START["세션 시작"] --> GRID["mapSizeX × mapSizeY 격자 생성"]
    GRID --> TIER["중심 거리로 Easy / Middle / Hard / Boss 등급 결정"]
    TIER --> PREFAB["필드 프리팹 무작위 선택"]
    PREFAB --> ROTATE["0° / 90° / 180° / 270° 회전"]
    ROTATE --> EVENT["탈출·아이템 타일 무작위 지정"]
    EVENT --> NAV["NavMesh 빌드"]
    NAV --> SPAWN["등급별 적 풀과 스폰 지점 선택"]
    SPAWN --> DISTRIBUTE["타일 단위로 프레임 분산 배치"]
    DISTRIBUTE --> PLAY["플레이 시작"]
```

맵은 완전한 절차적 메시 생성 대신, 검증된 타일 프리팹을 선택·회전·배치하는 방식입니다. 1인 개발 일정에서 제작과 검증 범위를 통제하면서 매 세션의 진행 경로와 이벤트 위치가 달라지도록 구성했습니다.

## Mermaid 원본

- [`client-flow.mmd`](client-flow.mmd)
- [`inventory-system.mmd`](inventory-system.mmd)
- [`skill-system.mmd`](skill-system.mmd)
- [`performance-flow.mmd`](performance-flow.mmd)
- [`map-generation-flow.mmd`](map-generation-flow.mmd)

## 관련 문서

- [성능 최적화 및 트러블슈팅](../technical/README.md)
- [프로젝트 루트 README](../../README.md)
