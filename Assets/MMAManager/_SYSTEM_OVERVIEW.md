# 🎯 MMA Manager - 완전한 비즈니게임 시스템

## 📊 프로젝트 구조

```
Assets/MMAManager/
├── Scripts/
│   ├── Models/              # Fighter, Match, Round, Event 데이터
│   ├── Systems/             # Career, Economy, Match Simulation, Training
│   ├── Visual/             # 3D Model Loader, Primitives
│   ├── ScriptableObjects/  # ScriptableObject 데이터 저장
│   ├── Test/               # 테스트 스크립트
│   └── Scenes/            # 게임 씬
```

---

## 🎮 핵심 시스템

### 1. MatchSimulationSystem
- ✅ 라운드 바이 봅매치 시뮬레이션
- 타격, 그래플링, 서브미션, KO 시뮬레이션
- 3라운 시뮬레이션 + 판정 시스템

### 2. TrainingSystem
- ✅ 주간 훈련 스케줄
- 8가지 훈련 프로그램 (스트렝스, 레슬링, BJJ, 주짓수 등)
- 피로도/컨디션에 따른 스탯 상승

### 3. CareerSystem
- ✅ 경영 진행 관리
- 챔피언십 순위: 브론즈 → 실버 → 골드 → 월드 챔피언십
- 4주일 매치 일정 + 타이틀 챔피언십 전
- 랜덤/랭커/미들급 챔피언십 생성

### 4. EconomySystem
- ✅ 경영 경제
- 매치 파이트 계산 (기본 + 승리 + KO/서브미션 보너스)
- 주간 비용 관리 (체육관, 코치, 훈련, 여행)
- 현금, 인기, 인기도, 스폰서십 보너스
- **스폰서 시스템**: 후원 회사 매치, 상금 승급 시 보너스

---

## 🎯 게임 플레이 로드맵

```
┌─────────────────────────────────────────────┐
│                                       │
│         MAIN MENU                   │
├─────────────────────────────────────────────┤
│                                       │
│  ┌─────────────┐    ┌─────────────┐    │
│  │ CAREER MODE  │    │ PROMOTION MODE  │    │
│  │              │    │                  │    │
│  │ • New Career  │    │ • Defeat         │    │
│  │ • Load Career  │    │ • Title Fight     │    │
│  │ • Rankings     │    │ • Tournament     │    │
│  │              │    │                  │    │
│  └─────────────┘    └────────────────┘    │
│                                       │
│  ┌─────────────┐    ┌─────────────┐    │
│  │ QUICK MATCH   │    │ MINI GAMES     │    │
│  │ • Sparring    │    │ • Training       │    │
│  │ • Exhibition  │    │ • Sparring       │    │
│  │              │    │                  │    │
│  └─────────────┘    └────────────────┘    │
│                                       │
│         [VISUAL MATCH VIEWER]          │
├─────────────────────────────────────────────┤
│                                       │
│   3D 파이터 모델                    │
│   • Remy.fbx (81개 애니메이션)     │
│   • 매치 시뮬레이션                   │
│                                       │
└─────────────────────────────────────────────┘
```

---

## 📖 구현된 기능

| 시스템 | 기능 | 상세 |
|--------|------|------|
| **CareerSystem** | 경영 진행, 챔피언십 순위, 매치 일정 |
| **EconomySystem** | 현금, 수입/지출, 스폰서, 인기/경험 |
| **MatchSimulationSystem** | 라운드 시뮬레이션, 타격/그래플링/서브미션 |
| **TrainingSystem** | 8가지 훈련 프로그램, 피로도 관리 |
| **Fighter Model** | 8개 스탯 (STR, TEC, SPD, STA, DEF, WREST, BJJ) + 조건(컨디션, 피로도) |

---

이 모든 시스템이 상호 연결되어 완전한 **MMA Manager** 게임을 위한 토대입니다!

**다음 구현 단계:**
1. 🔧 매치 매치 시뮬레이션 UI
2. 🏋️️ 경영 씬(스케줄 선택, 매치 결과 표시)
3. 📺 통계 화면(전적, 승률, 수익)
4. 🎁 훈련 씬(스탯 선택, 훈련 결과 표시)
5. 🏪 스폰서(스폰서, 계약 관리)

**준비 완료! 코드 컴파일 후 바로 테스트 가능합니다.**
