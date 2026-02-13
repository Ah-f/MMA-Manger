# Google Drive MCP 설치 가이드

## 1. Google Cloud 설정 (완료 필요)

### 1.1 OAuth Consent Screen 설정
```
Google Cloud Console → APIs & Services → OAuth consent screen
```

**필드 입력:**
- App name: `MMA Project Planner`
- User support email: 본인 이메일
- Developer contact: 본인 이메일
- Audience: `External`

**Scopes 추가 (Data Access):**
```
./auth/drive.file
./auth/documents
./auth/spreadsheets
./auth/presentations
./auth/drive
./auth/drive.readonly
```

**Test users:** 본인 이메일 추가

### 1.2 OAuth 2.0 Credentials 생성
```
Google Cloud Console → APIs & Services → Credentials
```

**설정:**
- Application type: **Desktop app** (중요!)
- Name: `MMA Project Planner`
- Create 클릭

JSON 파일 다운로드 후 이름 변경: `gcp-oauth.keys.json`

### 1.3 API 활성화 확인
```
Google Cloud Console → APIs & Services → Library
```

활성화된 API 목록:
- ✅ Google Drive API
- ✅ Google Docs API
- ✅ Google Sheets API
- ✅ Google Slides API

---

## 2. MCP 서버 설치 (Windows)

### 방법 A: npx 사용 (권장)
```bash
# 자동 설치 및 실행
npx @piotr-agier/google-drive-mcp
```

### 방법 B: 로컬 설치
```bash
# 클론 및 설치
git clone https://github.com/piotr-agier/google-drive-mcp.git
cd google-drive-mcp
npm install

# OAuth 파일 설정
copy gcp-oauth.keys.example.json gcp-oauth.keys.json

# 인증 실행
npm run auth
```

---

## 3. Claude Desktop 설정

### 설정 파일 위치
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

### npx 사용 설정
```json
{
  "mcpServers": {
    "google-drive": {
      "command": "npx",
      "args": ["@piotr-agier/google-drive-mcp"],
      "env": {
        "GOOGLE_DRIVE_OAUTH_CREDENTIALS": "C:\\Users\\LG\\gcp-oauth.keys.json"
      }
    }
  }
}
```

### 로컬 설치 사용 설정
```json
{
  "mcpServers": {
    "google-drive": {
      "command": "node",
      "args": ["C:\\Users\\LG\\google-drive-mcp\\dist\\index.js"],
      "env": {
        "GOOGLE_DRIVE_OAUTH_CREDENTIALS": "C:\\Users\\LG\\gcp-oauth.keys.json"
      }
    }
  }
}
```

---

## 4. 인증 절차

1. **gcp-oauth.keys.json** 파일 준비
2. npx 명령 실행: `npx @piotr-agier/google-drive-mcp`
3. 브라우저가 열리고 Google 로그인
4. 권한 승인
5. 인증 완료 (토큰 자동 저장)

---

## 5. 기획 문서 작성 계획

### Google Docs 기획 문서 구성
```
1. 프로젝트 개요
   - MMA 3D 싸움 시뮬레이션
   - 구현 완료된 기능
   - 남은 작업

2. 시스템 아키텍처
   - RoundManager (라운드 시스템)
   - FighterAgent (AI 판정)
   - Combat3DManager (메인 매니저)
   - FightHUD (HP UI)

3. 스탯 시스템
   - HP 계산 공식
   - 데미지 계산 공식
   - 방어 확률 계산
   - 판정 간격 계산

4. 애니메이션 시스템
   - 공격 모션 (Jab, Hook, Kick)
   - 피격 모션 (Head Hit, Body Hit)
   - 방어 모션 (Center Block, Left/Right Block)
   - 이동 모션 (Step Forward/Backward)

5. 테스트 결과
   - 버그 현황
   - 해결된 문제
   - 개선 사항

6. 다음 작업 계획
   - MMAAssetsTest.cs 재작성
   - 파이터 로직 추가 (이동, 거리 체크)
   - 애니메이션 타이밍 조정
```

### Google Sheets 데이터 추적 계획
```
시트 1: 파이터 스탯
- 파이터 이름
- STR, TEC, SPD, STA, DEF, WREST, BJJ
- HP (80 + STA/3 + DEF/4)
- 데미지 범위 (3~10)

시트 2: 전투 결과 로그
- 라운드 번호
- 공격자
- 방어자
- 애니메이션 종류
- 데미지
- HP 변화

시트 3: 버그 추적
- 발생 시간
- 에러 메시지
- 해결 방법
- 상태 (해결/진행중/보류)
```
