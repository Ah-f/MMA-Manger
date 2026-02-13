# MMA Manager Gap Analysis (vs MVP v3.1-lite)

기준 문서: `MVP_CUT_v3.1-lite.md`  
분석 대상: `Assets/MMAManager/Scripts`

상태 기준:
- `완료`: MVP 요구를 충족하며 즉시 사용 가능
- `부분`: 뼈대는 있으나 MVP 완료 기준 미충족
- `없음`: 구현 부재 또는 플레이 루프에 연결되지 않음

---

## 1) 요약 결론
- 현재 상태는 `핵심 시스템 프로토타입은 존재`하지만, `MVP 플레이 루프 연결`이 부족합니다.
- 특히 `주간 시간 진행`, `부업 3종`, `저장/로드`, `UI 상호작용`, `핵심 버그`가 막혀 있어 지금 기준으로는 MVP 완료 불가입니다.
- 우선 `P0` 6개를 먼저 고치면 1시즌 플레이 가능한 형태로 진입할 수 있습니다.

---

## 2) 갭 분석표

| 영역 | MVP 요구 | 현재 상태 | 근거 | 갭/리스크 | 우선순위 | 다음 액션 |
|---|---|---|---|---|---|---|
| 게임 시작 플로우 | 새 커리어 시작/퀵매치 진입 가능 | 부분 | `Assets/MMAManager/Scripts/MainGameManager.cs:124`, `Assets/MMAManager/Scripts/MainGameManager.cs:156`, `Assets/MMAManager/Scripts/MainGameManager.cs:126`, `Assets/MMAManager/Scripts/MainGameManager.cs:158` | null 체크 조건이 반대로 작성되어 핵심 진입이 막힘 | P0 | `!= null`을 `== null`로 수정 후 시작 플로우 E2E 검증 |
| 주간 시간 루프 | 월~일 자동 진행 + 주간 결산 | 없음 | `Assets/MMAManager/Scripts/Systems/TrainingSystem.cs:138`, `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:127` | 훈련/경제 함수는 있으나 둘을 묶는 Week Orchestrator 부재 | P0 | `TimeProgressionSystem` 추가(주간 계획→실행→결산) |
| 훈련 시스템 | 주간 훈련/휴식/강도 반영 | 부분 | `Assets/MMAManager/Scripts/Systems/TrainingSystem.cs:86`, `Assets/MMAManager/Scripts/Systems/TrainingSystem.cs:138` | 동작은 있으나 코치 보정/부업 페널티/UI 연결 부족 | P1 | 주간 스케줄 UI와 통합, 결과 리포트 생성 |
| 선수 핵심 스탯 | STR/TEC/SPD/STA/DEF/WRW/BJJ/POT | 완료 | `Assets/MMAManager/Scripts/Models/Fighter.cs:93`, `Assets/MMAManager/Scripts/Models/Fighter.cs:94` | MVP 축소 요구 충족 | P2 | 수치 밸런싱만 진행 |
| 성격/사기(축소 3종) | 투사형/분석형/스타형 + Morale | 없음 | `Assets/MMAManager/Scripts/Models/Fighter.cs:330` | Trait enum은 있으나 성격/사기 로직 없음 | P1 | `PersonalityType`, `Morale` 필드와 주간 증감 규칙 구현 |
| 경기 시뮬 | 3R + KO/TKO/Submission/Decision | 부분 | `Assets/MMAManager/Scripts/Systems/MatchSimulationSystem.cs:68`, `Assets/MMAManager/Scripts/Systems/MatchSimulationSystem.cs:458` | 결과 타입은 있으나 라운드 채점값 누적이 비어 Decision 품질 낮음 | P0 | `fighter1SignificantStrikes` 등 라운드 통계 누적 로직 보강 |
| 경기 전술(3축) | 타격/밸런스/그래플링 + 공격성/체력 운용 | 부분 | `Assets/MMAManager/Scripts/Systems/MatchSimulationSystem.cs:51`, `Assets/MMAManager/Scripts/Combat/FighterAgent.cs:301` | AI 전투는 있으나 유저가 선택하는 전술 UI/파라미터가 없음 | P1 | `FightTacticProfile` 데이터 + 매치 시작 전 선택 UI 추가 |
| 경제 루프 | 수입/지출/주간 손익 | 부분 | `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:44`, `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:107`, `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:127` | 파이트머니/비용은 있으나 MVP 핵심인 부업 수입이 미구현 | P0 | 부업 수입원을 EconomySystem으로 통합 |
| 부업 3종 | 예능/해설/유튜브 + 훈련 페널티 | 없음 | `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:170` | sponsor 보너스만 존재, 부업 도메인 객체/스케줄/이벤트 없음 | P0 | `ManagerSideJobSystem` 신설(3종 + 일수/페널티/인지도) |
| 발굴(2경로) | 아마추어/지역체육관 2경로 + 계약 | 부분 | `Assets/MMAManager/Scripts/Systems/ScoutingSystem.cs:40`, `Assets/MMAManager/Scripts/Systems/ScoutingSystem.cs:113` | 8경로는 있으나 계약/UI/경제 연동이 약함 | P1 | MVP용 2경로 preset + 영입 비용 연결 |
| 랭킹/매칭 | 체급+랭킹 후보 매칭 | 부분 | `Assets/MMAManager/Scripts/Systems/CareerSystem.cs:158`, `Assets/MMAManager/Scripts/Systems/CareerSystem.cs:174` | popularity 기반 단순 랭크만 존재, 매치메이킹 규칙 미완성 | P1 | 체급 필터 + 랭킹 범위 후보 3인 제시 로직 구현 |
| 파산/실패 조건 | 잔고<0 4주 지속 경고/실패 | 없음 | `Assets/MMAManager/Scripts/Systems/EconomySystem.cs:127` | 손익 계산만 있고 파산 상태머신 없음 | P0 | `negativeCashWeeks` 카운터 + 경고/종료 이벤트 추가 |
| 저장/로드 | 주간 단위 지속 저장 | 부분 | `Assets/MMAManager/Scripts/MainGameManager.cs:81`, `Assets/MMAManager/Scripts/Systems/FighterDatabase.cs:198`, `Assets/MMAManager/Scripts/Systems/FighterDatabase.cs:209` | TODO/빈 메서드 상태로 실사용 불가 | P0 | JSON 저장 스키마 정의 후 주간 autosave 구현 |
| UI 상호작용 | 홈/선수/주간계획/경기/재정 화면 | 부분 | `Assets/MMAManager/Scripts/UI/GameSetupScreen.cs:337`, `Assets/MMAManager/Scripts/UI/GameSetupScreen.cs:153` | 커스텀 ButtonEvents만 있고 Unity Button 클릭 체인이 불완전 | P0 | `UnityEngine.UI.Button` 기반 이벤트 바인딩으로 교체 |
| 알림 시스템 | 부상/경기/계약 경고 | 부분 | `Assets/MMAManager/Scripts/UI/EventSystem.cs:52` | Notification 생성은 있으나 도메인 이벤트 연결 부족 | P2 | 시스템 이벤트 버스와 연결 |

---

## 3) P0 즉시 작업 목록 (MVP 진입 차단 해소)
1. `MainGameManager` null 체크 버그 수정  
2. 주간 진행 오케스트레이터(`TimeProgressionSystem`) 추가  
3. `ManagerSideJobSystem` (예능/해설/유튜브) 구현 및 Economy 연동  
4. 저장/로드 최소 구현(JSON autosave)  
5. GameSetup UI를 실제 `Button` 클릭 구조로 교체  
6. MatchSimulation 라운드 스코어 누적 보강

---

## 4) 권장 2주 실행 순서
1. Week A: `P0-1,2,4` (플레이 루프 성립)
2. Week A: `P0-5` (UI 조작 가능화)
3. Week B: `P0-3` (부업 트레이드오프 핵심 재미)
4. Week B: `P0-6` (전투 결과 신뢰도 보강)

---

## 5) 현재 강점 (유지 권장)
- 전투/훈련/발굴/경제 도메인 코드가 이미 분리되어 있어 확장 기반은 좋음
- 3라운드 전투와 주요 스탯 체계는 MVP 축에 맞는 골격이 이미 존재
- 테스트 스크립트가 따로 있어 리팩터링 시 회귀 점검 기반 확보 가능
