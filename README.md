<div align="center">

# ASH:BORN

### Unity 모바일 탑다운 액션 게임 · 1인 개발

전투·성장·파밍을 거쳐 제한 시간 안에 탈출하는 모바일 탑다운 액션 게임입니다.

![Unity 6](https://img.shields.io/badge/Unity-6-000000)
![CSharp](https://img.shields.io/badge/CSharp-Client-512BD4)
![Android](https://img.shields.io/badge/Android-Mobile-3DDC84)
![WebGL](https://img.shields.io/badge/WebGL-Demo-F16529)
![Solo Development](https://img.shields.io/badge/Development-Solo-orange)
![Google Play Release Process](https://img.shields.io/badge/Google_Play-Production_Review-414141)

**Google Play 비공개 테스트 완료 · 프로덕션 출시 심사 진행**

[WebGL 데모](https://lookiesr.itch.io/ashborn) ·
[게임플레이 시스템](#gameplay-systems) ·
[클라이언트 구조](#client-architecture) ·
[런타임 성능](#runtime-performance) ·
[렌더링 프로파일링](#rendering-profiling) ·
[트러블슈팅](#troubleshooting) ·
[코드 맵](#code-map)

![ASH:BORN combat and skill progression gameplay](Docs/screenshots/portfolio/composite/combat-flow.jpg)

</div>

이 저장소는 ASH:BORN에서 직접 구현한 **Unity 클라이언트 코드와 기술 문서**를 정리한 포트폴리오입니다. 상용 에셋과 배포 관련 파일은 제외되어 있으며, 실제 플레이는 WebGL 데모에서 확인할 수 있습니다.

## 프로젝트 한눈에 보기

| 항목 | 내용 |
|---|---|
| 역할 | 기획부터 Unity 클라이언트 구현과 빌드까지 1인 개발 |
| 주요 시스템 | Combat, Enemy AI, Skill, Inventory, Equipment, QuickSlot, Save, UI |
| 기술·빌드 | Unity 6 (6000.2.4f1), C#, Android, WebGL |
| 출시 상태 | Google Play 비공개 테스트 완료 · 프로덕션 출시 심사 진행 |

## 이 프로젝트에서 보여주고 싶은 것

### 01. 게임플레이 시스템을 끝까지 연결한 경험

전투와 적 AI, 스킬 성장, 아이템 파밍, 인벤토리·장비, 저장, 탈출과 결과 정산까지 하나의 플레이 루프로 구현했습니다.

[EnemyController](Assets/Scripts/Enemy/EnemyController.cs) ·
[SkillManager](Assets/Scripts/Managers/SkillManager.cs) ·
[InventoryManager](Assets/Scripts/Managers/InventoryManager.cs) ·
[PlayerDataManager](Assets/Scripts/Managers/PlayerDataManager.cs)

### 02. 런타임 비용을 줄이면서 생긴 문제까지 다시 고친 경험

Object Pooling, Enemy 거리 검사 분할, Grid 기반 Tile 활성화를 적용했습니다. 단순히 기능을 넣는 데서 끝내지 않고, 분할 Scan이 중간에 멈추거나 Active Tile이 누적되는 문제를 실제 플레이 중 확인해 다시 수정했습니다.

[ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs) ·
[EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs) ·
[MapManager](Assets/Scripts/Managers/MapManager.cs)

### 03. Profiler로 확인하고, 별도 테스트에서 적용 조건을 검증한 경험

Profiler와 Frame Debugger로 실제 InGame 렌더링 상태를 확인하고, GPU Instancing은 별도 Benchmark Scene에서 동일 Mesh/Material 조건으로 A/B 테스트했습니다.

[렌더링 프로파일링](#rendering-profiling) ·
[상세 기술 문서](Docs/technical/README.md)

<a id="gameplay-systems"></a>
## 게임플레이 시스템

![ASH:BORN chest interaction and inventory transfer flow](Docs/screenshots/portfolio/composite/interaction-flow.jpg)

| 플레이 경험 | 구현 내용 | 코드 |
|---|---|---|
| Combat · Enemy AI | 플레이어 상태와 피해 처리, 적 탐지·이동·공격 행동 구성 | [Player](Assets/Scripts/Player/Player.cs), [EnemyController](Assets/Scripts/Enemy/EnemyController.cs), [Enemy Nodes](Assets/Scripts/Enemy/Node) |
| Skill | ScriptableObject 데이터에서 `ISkill` 런타임 생성, 레벨·마나 관리 | [SkillManager](Assets/Scripts/Managers/SkillManager.cs), [ISkill](Assets/Scripts/Interface/ISkill.cs) |
| Item | Inventory는 실제 아이템, Equipment는 장착 상태, QuickSlot은 GUID 참조를 관리 | [InventoryManager](Assets/Scripts/Managers/InventoryManager.cs), [EquipmentManager](Assets/Scripts/Managers/EquipmentManager.cs), [QuickSlotManager](Assets/Scripts/Managers/QuickSlotManager.cs) |
| Save | 골드·아이템·장비·퀵슬롯·영구 성장 데이터를 JSON으로 저장 | [PlayerDataManager](Assets/Scripts/Managers/PlayerDataManager.cs), [PlayerWallet](Assets/Scripts/Player/PlayerWallet.cs) |
| 상호작용 · 탈출 | 상호작용 계약과 상자, 탈출, 결과 흐름 연결 | [IInteractable](Assets/Scripts/Interface/IInteractable.cs), [ChestInteractable](Assets/Scripts/Object/ChestInteractable.cs), [EscapePortal3D](Assets/Scripts/Object/EscapePortal.cs) |

<a id="client-architecture"></a>
## 클라이언트 구조

구조를 나눌 때는 **누가 상태를 소유하는지**를 기준으로 잡았습니다.

- `GameManager`는 씬과 상위 게임 상태를 관리합니다.
- `InGameManager`는 한 번의 던전 세션과 결과 흐름을 관리합니다.
- `ItemDataSO`는 정적 원형 데이터, `ItemInstance`는 GUID를 가진 런타임 아이템입니다.
- `Inventory`, `Equipment`, `QuickSlot`은 UI 모양이 아니라 실제 소유권과 규칙에 따라 분리했습니다.
- HUD와 Inventory·QuickSlot의 주요 갱신은 상태 변경 이벤트를 사용합니다.

[전체 Client · Item · Skill · Map 다이어그램](Docs/architecture/README.md)

<a id="runtime-performance"></a>
## 런타임 성능

### 오브젝트 풀링

Enemy, Projectile, VFX처럼 전투 중 반복해서 쓰는 객체는 `ObjectPoolManager`에서 재사용합니다.

적은 단순히 `SetActive(true)`만 해서 다시 쓰지 않습니다. 이전 전투의 체력, Collider, AI 상태, Target, 사망 처리 플래그가 남을 수 있기 때문에 `OnSpawnInitialize`에서 다시 사용할 상태를 명시적으로 초기화합니다.

[ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs) ·
[PooledObject](Assets/Scripts/Object/PooledObject.cs) ·
[EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs) ·
[EnemyController](Assets/Scripts/Enemy/EnemyController.cs)

### Enemy Multi-frame Scan

적이 많아질수록 거리 검사가 한 프레임에 몰리는 것을 피하려고 `checksPerFrame` 단위로 나눠 검사하도록 만들었습니다.

처음 구현에서는 여기서 문제가 생겼습니다. 첫 chunk를 처리한 뒤 플레이어의 마지막 위치를 바로 갱신했기 때문에, 플레이어가 멈추면 아직 검사하지 않은 Enemy가 남아도 다음 chunk가 실행되지 않았습니다.

```text
플레이어 이동
→ 첫 chunk 검사
→ 마지막 위치 갱신
→ 플레이어 정지
→ 남은 Enemy 검사 중단
```

이동 감지와 현재 Scan의 진행 상태를 분리해, 한 번 시작한 Scan은 cursor가 전체 대상을 처리할 때까지 여러 프레임에 걸쳐 계속 진행하도록 바꿨습니다.

`EnemyCulling.ScanChunk` ProfilerMarker와 Development Build 진단 값으로 진행 상태와 완료 cycle을 확인할 수 있습니다.

[EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs) ·
[상세 기록](Docs/technical/README.md#1-enemy-multi-frame-scan-stopping-after-the-first-chunk)

### Grid-based Tile Activation

플레이어를 이동시키며 디버그 값을 확인하던 중 Active Tiles가 다음처럼 계속 늘어나는 현상을 확인했습니다.

```text
4 → 9 → 14 → 15
```

의도는 플레이어 주변 Tile만 활성 상태로 유지하는 것이었지만, 기존 구현은 새 위치 주변의 Tile만 검사하고 **이전 위치에서 활성화된 Tile을 새 반경 밖에서 끄지 못하고 있었습니다.**

그래서 현재 필요한 Tile과 이미 활성화된 Tile을 각각 `HashSet`으로 관리하도록 바꿨습니다.

```text
현재 활성 - 현재 필요 → Disable
현재 필요 - 현재 활성 → Enable
```

Scene Gizmo에는 현재 Grid, 활성 반경, 활성 Tile을 표시해 이동하면서 상태를 바로 확인할 수 있게 했습니다. 위 `4 → 9 → 14 → 15`는 수정 전 문제를 확인했을 때의 기록입니다.

[MapManager](Assets/Scripts/Managers/MapManager.cs) ·
[Tile](Assets/Scripts/Map/Tile.cs) ·
[상세 기록](Docs/technical/README.md#2-grid-active-tiles-accumulating-during-movement)

### 적 생성 작업 분산

던전 진입 시 모든 Tile의 적 배치 작업이 한 프레임에 몰리지 않도록 `CoSpawnAllEnemies`에서 Tile 단위로 작업한 뒤 `yield return null`로 다음 프레임에 이어갑니다.

[EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs)

### 이벤트 기반 UI

Inventory 변경, 아이템 이동·삭제, HP·EXP·Mana·Gold, Timer처럼 상태가 바뀌는 지점에서 UI가 갱신되도록 구성했습니다. 일부 Equipment·Window 흐름은 명시적 refresh를 유지합니다.

[InventoryManager](Assets/Scripts/Managers/InventoryManager.cs) ·
[PlayerHUDController](Assets/Scripts/UI/PlayerHUDController.cs) ·
[InGameHUDController](Assets/Scripts/UI/InGameHUDController.cs)

<a id="rendering-profiling"></a>
## 렌더링 프로파일링

렌더링 최적화는 먼저 실제 InGame 상태를 확인한 뒤, 원인을 분리할 필요가 있는 항목만 별도 테스트로 검증했습니다.

```text
Profiler → Frame Debugger → Controlled A/B Test → 적용 여부 결정
```

| 측정 | 조건 | 기록 |
|---|---|---:|
| InGame baseline | 실제 InGame 화면 | Batches 63 · SetPass 37 · Triangles 24.1k · Vertices 48.7k |
| GPU Instancing benchmark | 동일 Mesh/Material · 64 MeshRenderer · Windows Editor / DX12 | Batches 67 → 4 |

GPU Instancing의 `67 → 4`는 **별도 Benchmark Scene에서 동일 조건으로 비교한 결과**입니다. 실제 게임 전체의 Batches가 67에서 4로 줄었다는 의미는 아닙니다.

현재 저장소에는 해당 Profiler·Frame Debugger 원본 캡처가 포함되어 있지 않아, 정량 수치는 위 측정 기록까지만 사용하고 있습니다.

[상세 측정 해석](Docs/technical/README.md#rendering-profiling)

<a id="troubleshooting"></a>
## 트러블슈팅

기능 구현 이후 실제 플레이와 디버깅 과정에서 다시 손본 사례들입니다.

| 사례 | 무엇이 문제였나 | 어떻게 바꿨나 |
|---|---|---|
| Enemy Multi-frame Scan | 첫 chunk 이후 플레이어가 멈추면 남은 Enemy 검사가 진행되지 않음 | 이동 감지와 Scan 진행 상태를 분리하고 전체 대상 완료까지 cursor 유지 |
| Grid Active Tiles | 이동할수록 활성 Tile이 누적됨 | 현재 활성 집합과 필요한 집합을 비교해 Enable / Disable |
| Pooled Enemy Lifecycle | 재사용한 Enemy에 이전 체력·AI·Target·사망 상태가 남을 수 있음 | `OnSpawnInitialize`에서 재사용 상태를 명시적으로 초기화 |
| Item Ownership | Inventory·Equipment·QuickSlot을 하나의 슬롯 규칙으로 처리하기 어려움 | 실제 소유와 참조 책임을 분리하고 QuickSlot은 GUID로 추적 |
| Replayable Dungeon | 완전 절차 생성은 1인 개발 범위에서 비용이 큼 | 검증된 Tile prefab을 조합해 세션마다 배치와 진행 경로 변화 |

각 사례의 코드 흐름과 수정 이유는 [상세 기술 문서](Docs/technical/README.md#troubleshooting)에 더 자세히 정리했습니다.

![ASH:BORN extraction and result flow](Docs/screenshots/portfolio/composite/extraction-flow.jpg)

<a id="code-map"></a>
## 코드 맵

| 보고 싶은 영역 | 시작 코드 |
|---|---|
| 게임·세션 | [GameManager](Assets/Scripts/Managers/GameManager.cs), [InGameManager](Assets/Scripts/Managers/InGameManager.cs) |
| Combat · Enemy AI | [EnemyController](Assets/Scripts/Enemy/EnemyController.cs), [EnemyDetection](Assets/Scripts/Enemy/EnemyDetection.cs), [Enemy Nodes](Assets/Scripts/Enemy/Node) |
| Skill | [SkillManager](Assets/Scripts/Managers/SkillManager.cs), [ISkill](Assets/Scripts/Interface/ISkill.cs) |
| Item | [InventoryManager](Assets/Scripts/Managers/InventoryManager.cs), [EquipmentManager](Assets/Scripts/Managers/EquipmentManager.cs), [QuickSlotManager](Assets/Scripts/Managers/QuickSlotManager.cs) |
| Pool · Enemy Lifecycle | [ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs), [EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs), [EnemyController](Assets/Scripts/Enemy/EnemyController.cs) |
| Enemy Scan · Grid Activation | [EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs), [MapManager](Assets/Scripts/Managers/MapManager.cs) |
| UI 갱신 | [PlayerHUDController](Assets/Scripts/UI/PlayerHUDController.cs), [InGameHUDController](Assets/Scripts/UI/InGameHUDController.cs) |

## 더 자세한 문서

- [상세 기술 문서](Docs/technical/README.md) — 런타임 성능, 렌더링 측정, 트러블슈팅
- [클라이언트 구조](Docs/architecture/README.md) — Client, Inventory, Skill, Map 다이어그램
- [스크린샷 목록](Docs/screenshots/portfolio/README.md) — README와 포트폴리오에서 사용하는 이미지

> 이 저장소에는 직접 구현한 공개용 C# 코드와 기술 문서를 담았습니다. 라이선스가 있는 에셋, 전체 Scene·Prefab, 광고·스토어 배포 설정과 민감 정보는 포함하지 않습니다.
