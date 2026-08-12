<div align=center>

# ASH:BORN

### Unity 모바일 탑다운 액션 게임 · 1인 개발

전투·성장·파밍을 거쳐 제한 시간 안에 탈출하는 모바일 탑다운 액션 게임입니다.
이 저장소는 실제 프로젝트의 **공개 가능한 Unity 클라이언트 코드와 기술 문서**만 선별한 포트폴리오입니다.

![Unity 6](https://img.shields.io/badge/Unity-6-000000)
![CSharp](https://img.shields.io/badge/CSharp-Client-512BD4)
![Android](https://img.shields.io/badge/Android-Mobile-3DDC84)
![WebGL](https://img.shields.io/badge/WebGL-Demo-F16529)
![Solo Development](https://img.shields.io/badge/Development-Solo-orange)
![Google Play Release Process](https://img.shields.io/badge/Google_Play-Production_Review-414141)

**Google Play 비공개 테스트 완료 · 프로덕션 출시 심사 진행**

[WebGL 데모](https://lookiesr.itch.io/ashborn) · [게임플레이 시스템](#gameplay-systems) · [클라이언트 구조](#client-architecture) · [런타임 성능](#runtime-performance) · [렌더링 프로파일링](#rendering-profiling) · [트러블슈팅](#troubleshooting) · [코드 맵](#code-map) · [상세 기술 문서](Docs/technical/README.md)

![ASH:BORN combat and skill progression gameplay](Docs/screenshots/portfolio/composite/combat-flow.jpg)

</div>

## 프로젝트 한눈에 보기

| 항목 | 내용 |
|---|---|
| 역할 | 기획부터 Unity 클라이언트 구현과 빌드까지 1인 개발 |
| 주요 시스템 | Combat, Enemy AI, Skill, Inventory, Equipment, QuickSlot, Save, UI |
| 기술·빌드 | Unity 6 (6000.2.4f1), C#, Android, WebGL |
| 출시 상태 | Google Play 비공개 테스트 완료 · 프로덕션 출시 심사 진행 |
| 공개 범위 | C# 코드·설계 문서·직접 캡처한 화면; 라이선스 에셋과 배포 설정 제외 |

> WebGL 데모는 브라우저에서 실행할 수 있습니다. 이 저장소는 라이선스 에셋·씬·프리팹을 제외한 코드 포트폴리오이므로 단독 실행용 전체 Unity 프로젝트가 아닙니다.

## 핵심 엔지니어링

### 01 게임플레이 시스템

Combat과 Enemy AI, 런타임 Skill 생성·레벨업, Inventory·Equipment·QuickSlot, JSON Save, 탈출·사망 정산을 하나의 플레이 루프로 연결했습니다.
[EnemyController](Assets/Scripts/Enemy/EnemyController.cs) · [SkillManager](Assets/Scripts/Managers/SkillManager.cs) · [InventoryManager](Assets/Scripts/Managers/InventoryManager.cs) · [PlayerDataManager](Assets/Scripts/Managers/PlayerDataManager.cs)

### 02 클라이언트 구조

정적 원형 데이터와 런타임 상태를 분리하고 게임 흐름·도메인 규칙·UI 책임을 나눴습니다. Inventory는 실제 아이템, Equipment는 장착 상태, QuickSlot은 GUID 참조를 관리합니다.
[EquipmentManager](Assets/Scripts/Managers/EquipmentManager.cs) · [QuickSlotManager](Assets/Scripts/Managers/QuickSlotManager.cs) · [Architecture Diagrams](Docs/architecture/README.md)

### 03 런타임 성능

Enemy·Projectile·VFX 재사용, 전체 대상 처리를 보장하는 Enemy 거리 검사 분할, Grid Active Set 조정, Coroutine 기반 Spawn 분산, 이벤트 기반 UI 갱신을 적용했습니다.
[ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs) · [EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs) · [MapManager](Assets/Scripts/Managers/MapManager.cs) · [EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs)

### 04 렌더링 프로파일링

Profiler → Frame Debugger → 통제된 A/B 테스트 순서로 확인했습니다. InGame baseline과 64 MeshRenderer GPU Instancing benchmark를 분리해 기록합니다.

### 05 트러블슈팅

분할 Scan, Grid Active Set, 풀링 lifecycle, item ownership, 제한된 Tile 에셋의 재사용성을 같은 진단 형식으로 기록했습니다.
[상세 트러블슈팅](Docs/technical/README.md#troubleshooting)

<a id=gameplay-systems></a>
## 게임플레이 시스템

![ASH:BORN chest interaction and inventory transfer flow](Docs/screenshots/portfolio/composite/interaction-flow.jpg)

| 플레이 경험 | 구현 책임 | 코드 증거 |
|---|---|---|
| Combat·Enemy AI | 상태·피해 lifecycle과 탐지·이동·공격 행동 구성 | [Player](Assets/Scripts/Player/Player.cs), [EnemyController](Assets/Scripts/Enemy/EnemyController.cs), [Enemy Nodes](Assets/Scripts/Enemy/Node) |
| Skill | ScriptableObject에서 ISkill 런타임 생성, 레벨·마나 관리 | [SkillManager](Assets/Scripts/Managers/SkillManager.cs), [ISkill](Assets/Scripts/Interface/ISkill.cs) |
| Item | 실제 소유, 장착 상태, GUID 참조 분리 | [InventoryManager](Assets/Scripts/Managers/InventoryManager.cs), [EquipmentManager](Assets/Scripts/Managers/EquipmentManager.cs), [QuickSlotManager](Assets/Scripts/Managers/QuickSlotManager.cs) |
| Save | 골드·아이템·장비·퀵슬롯·영구 성장 JSON 저장 | [PlayerDataManager](Assets/Scripts/Managers/PlayerDataManager.cs), [PlayerWallet](Assets/Scripts/Player/PlayerWallet.cs) |
| 상호작용·탈출 | 공통 계약과 결과 흐름 | [IInteractable](Assets/Scripts/Interface/IInteractable.cs), [ChestInteractable](Assets/Scripts/Object/ChestInteractable.cs), [EscapePortal3D](Assets/Scripts/Object/EscapePortal.cs) |

<a id=client-architecture></a>
## 클라이언트 구조

- GameManager는 씬과 상위 상태, InGameManager는 한 번의 던전 세션을 관리합니다.
- ItemDataSO와 GUID를 가진 런타임 ItemInstance를 분리합니다.
- Inventory·Equipment·QuickSlot은 화면 모양이 아니라 소유권과 규칙으로 나눴습니다.
- 주요 HUD·Inventory·QuickSlot은 변경 이벤트를 사용합니다. 일부 Equipment·Window 흐름은 명시적 refresh를 유지합니다.

[전체 Client·Item·Skill·Map Mermaid 다이어그램](Docs/architecture/README.md)

<a id=runtime-performance></a>
## 런타임 성능

### A. 오브젝트 풀링 (Object Pooling)

**Problem →** Enemy·Projectile·VFX가 전투 중 반복 생성·파괴될 수 있었습니다.
**Observation →** 생성 주기가 짧고 재사용 가능한 객체를 공통 풀 대상으로 분류했습니다.
**Change →** poolId별 Queue, preload, Spawn/Despawn을 구성했습니다. Enemy 재사용 전 체력, Collider, Behavior Graph 변수, Target·탐지 참조, 사망 flag를 초기화합니다.
**Verification →** 재사용·반환과 reset 호출 경로를 코드에서 추적했습니다. 전체 Instantiate/Destroy 제거 또는 정량 향상은 주장하지 않습니다.

[ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs) · [PooledObject](Assets/Scripts/Object/PooledObject.cs) · [EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs) · [EnemyController](Assets/Scripts/Enemy/EnemyController.cs)

### B. Enemy Multi-frame Scan

**Problem →** 모든 Enemy의 거리 검사를 한 frame에 실행하면 작업이 집중될 수 있습니다.
**Observation →** 플레이어 이동 임계값을 넘으면 스캔을 시작하고, `checksPerFrame` 단위로 검사량을 나눕니다.
**Change →** 시작 위치 `_scanPlayerPosition`, 전체 대상 수 `_scanTargetCount`, 진행 상태 `_scanInProgress`, cursor `_currentIndex`를 유지해 전체 스캔이 끝날 때까지 여러 frame에 걸쳐 계속 처리합니다. null 항목도 처리 slot을 소비합니다.
**Verification →** `EnemyCulling.ScanChunk` ProfilerMarker와 Development Build 진단 값으로 chunk 진행·완료 cycle·활성 Enemy 수를 확인할 수 있습니다.

[EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs) · [상세 진단](Docs/technical/README.md#1-enemy-multi-frame-scan-stopping-after-the-first-chunk)

### C. Grid-based Tile Activation

**Problem →** 맵 전체 대신 플레이어 주변 Tile을 활성화 판단 단위로 관리할 필요가 있었습니다.
**Observation →** 이동 임계값, Grid 좌표, activeRadius / tileSize, 거리 제곱으로 후보를 계산합니다.
**Change →** `_desiredActiveTiles`와 `_activeTiles`를 분리하고 `current - desired`는 비활성화, `desired - current`는 활성화한 뒤 집합을 교체합니다.
**Verification →** `MapManager.UpdateActiveTiles` ProfilerMarker, Development Build 진단 값, Scene Gizmo에서 활성 Tile·반경·현재 Grid를 확인할 수 있습니다.

[MapManager](Assets/Scripts/Managers/MapManager.cs) · [Tile](Assets/Scripts/Map/Tile.cs) · [상세 진단](Docs/technical/README.md#2-grid-active-tiles-accumulating-during-movement)

### D. 적 생성 작업 분산

CoSpawnAllEnemies는 NavMesh 준비 후 Tile 하나를 처리할 때마다 yield return null로 다음 frame에 이어갑니다. CPU 감소 수치는 기재하지 않습니다.
[EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs)

### E. 이벤트 기반 UI

주요 HUD와 Inventory·QuickSlot은 Inventory, HP·EXP·Mana·Gold, Timer 변경 이벤트를 구독합니다. 모든 UI가 event-only라는 주장은 하지 않습니다.
[InventoryManager](Assets/Scripts/Managers/InventoryManager.cs) · [PlayerHUDController](Assets/Scripts/UI/PlayerHUDController.cs) · [InGameHUDController](Assets/Scripts/UI/InGameHUDController.cs)

<a id=rendering-profiling></a>
## 렌더링 프로파일링

**Profiler → Frame Debugger → 통제된 A/B 테스트 → 검증된 조건에만 적용**

| 측정 | 조건 | 기록 |
|---|---|---:|
| InGame baseline | 실제 InGame 화면 · 공개 저장소 외부 기록 | Batches 63 · SetPass 37 · Triangles 24.1k · Vertices 48.7k |
| GPU Instancing benchmark | 동일 Mesh/Material의 64 MeshRenderer · Windows Editor / DX12 · 공개 저장소 외부 기록 | Batches 67 → 4 |

동일 Mesh/Material 조건의 64 MeshRenderer 테스트에서 GPU Instancing 적용 전후 **Batches 67 → 4**를 확인했습니다. 이는 통제된 benchmark이며 **게임 전체 Batches가 67 → 4가 된 것이 아닙니다.** FPS, GC Alloc, CPU/GPU ms, memory 수치는 공개 근거가 없어 기재하지 않습니다.

위 수치는 공개 포트폴리오 준비 과정에서 저장소 외부에 기록된 측정값입니다. 현재 저장소에는 이를 직접 검증할 Profiler·Frame Debugger·Instancing 비교 캡처가 없습니다. [상세 측정 해석](Docs/technical/README.md#rendering-profiling)

<a id=troubleshooting></a>
## 트러블슈팅

| 순서 | 사례 | 현재 상태 |
|---:|---|---|
| 1 | Enemy multi-frame scan이 첫 chunk 후 멈춤 | scan state·captured position·cursor 수정 반영 |
| 2 | 이동 중 Grid active tiles 누적 | desired/current 집합 조정 반영 |
| 3 | 풀링 Enemy가 이전 lifecycle state 유지 | reset code 반영 |
| 4 | Inventory / Equipment / QuickSlot ownership 충돌 | ownership 분리 반영 |
| 5 | 제한된 Tile assets와 replayable dungeon | prefab 조합 방식 반영 |

각 사례의 **문제 → 관찰 / 진단 → 근본 원인 → 수정 → 검증**은 [상세 기술 문서](Docs/technical/README.md#troubleshooting)에 정리했습니다.

![ASH:BORN extraction and result flow](Docs/screenshots/portfolio/composite/extraction-flow.jpg)

<a id=code-map></a>
## 코드 맵

| 검토 영역 | 시작 코드 |
|---|---|
| 게임·세션 | [GameManager](Assets/Scripts/Managers/GameManager.cs), [InGameManager](Assets/Scripts/Managers/InGameManager.cs) |
| Combat·Enemy AI | [EnemyController](Assets/Scripts/Enemy/EnemyController.cs), [EnemyDetection](Assets/Scripts/Enemy/EnemyDetection.cs), [Enemy Nodes](Assets/Scripts/Enemy/Node) |
| Skill | [SkillManager](Assets/Scripts/Managers/SkillManager.cs), [ISkill](Assets/Scripts/Interface/ISkill.cs) |
| Item ownership | [InventoryManager](Assets/Scripts/Managers/InventoryManager.cs), [EquipmentManager](Assets/Scripts/Managers/EquipmentManager.cs), [QuickSlotManager](Assets/Scripts/Managers/QuickSlotManager.cs) |
| Pool·Enemy lifecycle | [ObjectPoolManager](Assets/Scripts/Managers/ObjectPoolManager.cs), [EnemySpawner](Assets/Scripts/Enemy/EnemySpawner.cs), [EnemyController](Assets/Scripts/Enemy/EnemyController.cs) |
| Enemy Scan·Grid Activation | [EnemyCullingManager](Assets/Scripts/Managers/EnemyCullingManager.cs), [MapManager](Assets/Scripts/Managers/MapManager.cs) |
| Event-driven UI | [PlayerHUDController](Assets/Scripts/UI/PlayerHUDController.cs), [InGameHUDController](Assets/Scripts/UI/InGameHUDController.cs) |

## 문서

- [상세 기술 문서](Docs/technical/README.md) — 런타임·렌더링·트러블슈팅 근거
- [클라이언트 구조](Docs/architecture/README.md) — Client·Inventory·Skill·Map 다이어그램
- [스크린샷 목록](Docs/screenshots/portfolio/README.md) — 공개 이미지 크기와 용도

## 공개 포트폴리오 범위

포함: 공개 가능한 Unity C# 코드, ScriptableObject 예시, 설계·기술 문서, 직접 캡처한 화면.
제외: 라이선스 assets, 모델·애니메이션·오디오, 전체 scenes·prefabs, credentials, 광고·스토어 배포 설정, private data.

**Release status:** Google Play 비공개 테스트 완료 · 프로덕션 출시 심사 진행. Google Play production release 완료 또는 live 상태로 표기하지 않습니다.
