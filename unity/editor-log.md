---
layout: page
title: "Unity 에디터 전용 로그 설계 — 컴파일 제거와 태그 필터"
permalink: /unity/editor-log/
tags:
  - unity
  - logging
  - conditional
  - performance
---

Unity에서 개발용 `Log` 래퍼를 직접 설계하는 경우가 많습니다. 편리하지만 잘못 두면 **개발 편의 코드가 릴리스 빌드 성능까지 갉아먹습니다.** 이 글은 로그를 세 번 다시 짜며 얻은 원칙을 정리합니다. 구현은 시간순으로 **P1**(가장 오래된 세대) → **P2** → **P3**(현재)로 부릅니다.

아래에서는 세대가 공유하는 기능을 먼저 보고, P1 → P2 → P3 순으로 각 세대만의 선택과 그 이유를 다룹니다.

---

## 공통 기능

세대마다 클래스 이름은 같지만(`Log`), 백엔드·빌드 비용 처리·API 형태는 달랐습니다. 다음은 **P1부터 P3까지 공통으로 유지한 개념**입니다.

### 로그 레벨

메시지마다 다섯 가지 레벨 중 하나를 고릅니다. 레벨별 on/off는 `GamePrefs`에 저장되어 에디터 세션을 넘어 유지됩니다.

| 레벨 | 용도 (예) | 콘솔 색상 (P3 기준) |
|------|-----------|---------------------|
| **Progress** | 진행·루프·임시 추적 (`Update` 디버그 등) | `#517348` |
| **Info** | 일반 상태·흐름 확인 | (기본) |
| **Warning** | 의심스러운 상태, 스팸성 경고 | `#E2B53E` |
| **Error** | 개발 중 발견한 오류 후보 | `#D92B2B` |
| **Except** | 예외·비정상 분기 추적 | `#252C59` |

P1·P2는 레벨 이름·색 구현이 조금 다르지만, **레벨로 출력을 걸러 보는** 목적은 같습니다.

### 태그 필터

`LogTags`로 시스템·기능 단위 필터를 겁니다. `LogTagFilter`(또는 P2의 `FindLog`)로 태그별 on/off를 제어하고, 레벨과 조합해 콘솔을 좁혀 봅니다.

### 출력 형태

출력은 대체로 `[태그] [레벨] 메시지` 형태에 레벨별 색이 입혀집니다. 레벨·태그가 off이면 콘솔에는 찍히지 않습니다. 다만 호출부에서 문자열·계산 비용이 나갈 수 있으며, 세대마다 이를 다르게 처리했습니다(「컴파일 타임 제거 vs 런타임 차단」 절을 참고하세요).

### 설정 API

`LoadLevel`, `SetLogLevelAll` / `SetLogLevelOff`, 레벨별 토글, `IsLevelEnabled` 등으로 레벨 on/off를 제어합니다. 세대마다 메서드 이름·저장 키는 조금 다르지만, **에디터에서 필터를 켜고 끄는 흐름**은 공통입니다.

---

## 세 번의 진화

| | 방향 |
|---|------|
| **P1** | `LogSystem` 기반, 릴리스에서 Error + 파일 로그 유지 |
| **P2** | P1 대비 기능 확장 — 태그 필터, 풍부한 오버로드, 스레드 안전, 색상 체계 |
| **P3** | P2 대비 역할 축소 — 에디터 전용 출력, API 최소화, `Debug.Log` 직접 사용 |

```text
P1  ──기능 확장──▶  P2  ──역할 분리·단순화──▶  P3
```

P1에서 쌓은 경험을 P2에서 프레임워크 수준으로 키웠고, 실전에서 드러난 비용 문제를 P3에서 구조적으로 해결한 흐름입니다. 세대별 전체 소스는 [소스 코드](#소스-코드) 절을 참고하세요.

---

## P1: 기본 래퍼와 릴리스 경로

P1은 **에디터 디버그**와 **릴리스 오류 수집**을 한 `Log` 클래스에 두는 것에서 출발했습니다.

### P1만의 특징

- **백엔드:** `LogSystem` — 빌드 종류에 따라 `Editor` / `Development` / `Client`로 초기화하고, 릴리스에서는 **파일 로그**를 켭니다.
- **빌드 비용:** 에디터 로그 대부분에 `[Conditional("UNITY_EDITOR")]` — `UNITY_EDITOR`가 없는 빌드에서는 해당 **호출문이 컴파일 단계에서 제거**됩니다.
- **릴리스 Error:** Conditional이 **없는** `Error` API — 플레이어 빌드에서도 콘솔 출력과 **파일 로그**가 남습니다.
- **API:** `params object[]` + `string.Format` 포맷 오버로드, 태그 있음/없음 변형.

```csharp
// P1 — 에디터 전용 (Conditional로 호출문 제거)
Log.Info(LogTags.Attack, "damage={0}", damage);

// P1 — 릴리스에도 남음 (Conditional 없음)
Log.Error("save failed: {0}", reason);
```

### 왜 이렇게 했는지

- 태그·레벨 필터로 에디터 콘솔을 좁혀 보는 흐름이 유용했습니다.
- 동시에 **실제 장애는 릴리스에도 기록**해야 해서 `Error`는 Conditional 밖에 두고 `LogSystem` 파일 로그와 연결했습니다.
- Conditional을 **일부 출력 API에만** 쓴 이유는, 당시에는 에디터 디버그 비용만 컴파일로 빼면 충분하다고 보았기 때문입니다. 릴리스용 `Error`는 의도적으로 남겼습니다.

**전체 구현:** [P1_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P1_Log.cs)

---

## P2: 기능 확장과 런타임 방어의 한계

P2는 P1에서 검증된 태그·레벨 필터를 바탕으로 **에디터 디버깅 UX**를 키우는 쪽으로 갔습니다.

### P2만의 특징

- **백엔드:** `LogSystem` 유지 (에디터는 파일 로그 off).
- **빌드 비용:** Conditional **없음** — `#if UNITY_EDITOR`, `IsLogLevelEnabled`, 태그 조회(`FindLog`) 등 **런타임·전처리기 방어**만 사용.
- **API 확장:** `params object[]` 오버로드를 늘리고, 레벨·태그 조합을 풍부하게 둡니다.
- **스레드 안전:** `lock`으로 동시 출력 보호.
- **색상:** `TSColors` 등으로 레벨별 색 체계 정리.

```csharp
// P2 — 로그가 꺼져 있어도 CalcDamage()와 인자 평가는 실행됩니다
Log.Info(LogTags.Attack, "damage={0}", CalcDamage(a, b));
```

### 왜 이렇게 했는지

- P1의 필터·태그가 쓸 만하다는 확신 → **호출 편의**를 우선해 오버로드를 늘렸습니다.
- P1에서 쓰던 Conditional을 빼고, 메서드 안에서 `if (!enabled) return`과 `#if`로 막으면 **플레이어 빌드 비용도 충분히 제어**된다고 보았습니다.
- 멀티스레드·비동기 경로에서 로그가 깨지지 않게 **스레드 안전**을 추가했습니다.

### P2에서 드러난 문제

P2는 태그·레벨 필터 덕분에 에디터 디버깅에 매우 유용했습니다. 문제는 **플레이어 빌드** 쪽이었습니다.

C#은 메서드를 호출하기 **전에** 인자를 모두 평가합니다. 로그 메서드 안에서 early return으로 막아도, 호출부에서 이미 비용이 나갑니다.

P2는 `IsLogLevelEnabled`가 `false`를 반환해 **출력만** 막았지만, **호출문은 바이너리에 남아** 다음 비용이 그대로 발생했습니다.

- `$""` 보간, `<color=...>` 문자열 조합
- ScriptableObject·설정 데이터 조회
- 로그용 값 계산
- `Update()` 등 **프레임마다** 호출되는 경로

로그가 콘솔에 찍히지 않아도 CPU·GC 할당이 쌓이면, 프로파일러에서 원인을 찾기 어렵습니다. P1이 일부 API에 쓰던 Conditional을 P2에서 되돌린 것이 핵심 회귀였습니다.

**전체 구현:** [P2_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P2_Log.cs)

### 컴파일 타임 제거 vs 런타임 차단

`[Conditional("UNITY_EDITOR")]`를 붙이면, `UNITY_EDITOR`가 없는 빌드에서는 **호출문 자체가 컴파일 단계에서 제거**됩니다. 인자 평가·문자열 생성·데이터 접근도 함께 빠집니다.

| 구분 | 시점 | 호출문 | 호출부 인자 평가 |
|------|------|--------|------------------|
| P1 | 컴파일 + 런타임 | 일부 출력 API만 제거; `Error` 등은 남음 | Conditional 적용 호출만 **제거됨** |
| P2: early return / `IsEnabled == false` / `#if` | 런타임 | 바이너리에 남음 | **실행됨** |
| P3: 플레이어 빌드 (출력 API) | **컴파일** | **제거됨** | **제거됨** |
| P3: 에디터 | — | 남음 | **실행됨** (레벨/태그 off여도 `$""`·계산은 호출 전) |

P2에서 “기능은 늘렸는데 **런타임 방어만으로는** 호출부 비용을 막지 못했다”는 교훈을 얻었습니다.

---

## P3: 역할 분리와 컴파일 제거

P3는 P2 교훈을 반영해 **출력은 에디터 전용**, **릴리스 메시지는 별도 경로**로 나눴습니다.

> **로그 출력은 에디터 전용. 릴리스 메시지는 `Debug.Log*` 등 별도 경로.**

### P3만의 특징

**출력 API**(`Log.Progress`, `Log.Info` 등)는 `[Conditional("UNITY_EDITOR")]`로 묶여 있어, `UNITY_EDITOR`가 없는 빌드에서는 **호출문이 컴파일 단계에서 제거**됩니다.

**설정·조회 API**(`LoadLevel`, `IsLevelEnabled`, `SetLevel`, `SwitchLevel*` 등)는 Conditional이 없어 플레이어 빌드에도 남습니다. 다만 플레이어 빌드에서는 출력 API 호출문이 없으므로 실질적인 로그 동작은 에디터에 한정됩니다. `LoadLevel`은 비에디터에서 레벨 기본값을 전부 `false`로 둡니다.

```csharp
#if UNITY_EDITOR
    bool defaultEnabled = true;
#else
    bool defaultEnabled = false;
#endif
```

- **백엔드:** `LogSystem` 제거 → `Debug.Log` + 색상만 사용. `LogWarning`/`LogError`는 쓰지 않습니다.
- **API:** 레벨마다 태그 없음 / `LogTags` 있음 두 오버로드만. 메시지는 완성된 `string` 하나(`$""` 보간은 호출부).
- **스레드:** 에디터 전용이라 P2의 `lock`은 제거했습니다.

```csharp
Log.Info("player spawned");
Log.Progress(LogTags.Damage, $"tick={tick}");
Log.Info(LogTags.Attack, $"damage={damage}");
```

### 왜 이렇게 했는지

**1. 출력 API 전면 Conditional**

P1에서 일부만 쓰던 컴파일 제거를, **에디터 전용 출력 API 전체**에 일관 적용했습니다. P2에서 배운 “런타임 방어로는 호출부 비용이 남는다”는 문제를 구조적으로 해결합니다.

```csharp
[Conditional("UNITY_EDITOR")]
public static void Info(LogTags tag, string message)
{
    Write(LogLevel.Info, tag, message);
}
```

**2. 에디터 전용 출력과 릴리스 경로 분리**

P1처럼 `Error` + 파일 로그를 `Log` 안에 유지하는 선택도 가능하지만, P3는 **역할 분리**를 택했습니다. 릴리스에 남길 메시지는 `Log` 래퍼 밖에서 `Debug.Log*` 등으로 처리합니다.

**3. P2에서 검증된 필터 유지**

`Write`에서 레벨·태그를 검사한 뒤에만 `Debug.Log`를 호출합니다.

```csharp
private static void Write(LogLevel level, LogTags? tag, string message)
{
    if (!IsLevelEnabled(level)) return;
    if (tag.HasValue && !LogTagFilter.IsTagEnabled(tag.Value)) return;

    // ...
    UnityEngine.Debug.Log(output);
}
```

**4. `params object[]` 제거, 호출부 포맷은 `$""`로 통일**

1차 목적은 **API 단순화**입니다. 오버로드를 줄이고, 로그 클래스 역할을 “필터 + 접두 + 색상 + 출력”으로 한정합니다. `params`를 없애면 박싱도 줄일 수 있지만, **P2 빌드에서 호출부 비용을 없앤 것은 `$""` 통일이 아니라 Conditional**입니다. `params`든 `$""`든 인자·보간은 호출 전에 평가됩니다(위 표 P3 에디터 행).

**5. `Debug.Log` 하나로 통일, 색상으로 레벨 구분**

에디터 로그는 **색상**으로만 레벨을 구분합니다(색상표는 「공통 기능」 절). 개발 중 출력과 실제 문제 보고를 **콘솔 아이콘만으로** 구분하지 않습니다.

구조화 로그(Unity Logging 패키지 등)도 선택지입니다. P3는 **에디터 개발 로그의 단순성**을 우선하고, 프로덕션 분석·수집은 별도 경로로 둡니다.

**에디터에서 무엇을 호출할지**는 여전히 개발자의 책임입니다. 레벨·태그 off여도 `$""` 보간 등 호출부 비용은 남을 수 있습니다.

**전체 구현:** [P3_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P3_Log.cs) (글의 스니펫은 이 파일에서 발췌). `LogTags`, `LogTagFilter`, `GamePrefs` 등 프로젝트 공통 타입은 repo에 포함되어 있지 않습니다.

---

## P1 · P2 · P3 비교 요약

| 항목 | P1 | P2 | P3 |
|------|----|----|-----|
| 백엔드 | `LogSystem` | `LogSystem` | `Debug.Log` |
| 릴리스 Error | 출력 유지 (+ 파일 로그) | 출력 비활성 (호출문 유지) | 출력 API 호출문 제거 |
| 빌드 비용 차단 | 일부 출력 API Conditional; `Error` 등은 런타임·파일 로그 | Conditional 없음 (런타임·`#if`만) | 출력 API 전면 Conditional |
| 태그 필터 | O | O | O |
| 포맷 오버로드 | O | O (다양) | X (호출부 `$""`) |
| 스레드 안전 | X | O (`lock`) | X (에디터 전용) |

---

## 실무 가이드라인

1. **가변적인 디버그 출력** → 에디터 전용 `Log.*`, 태그·레벨 활용
2. **릴리스에 남길 메시지** → `Debug.Log` / 리포팅, 개발용 `Log`와 분리
3. **프레임마다 도는 로그** → Progress·태그 off가 기본. 필요할 때만 잠깐 켜기
4. **무거운 문자열·계산** → 레벨/태그 off여도 `$""`·인자 평가는 실행됨. 핫 패스에서는 **호출 자체를 두지 않기**
5. **새 프로젝트** → 프로덕션 로그와 개발 로그 통로를 처음부터 분리. 구조화 로그는 별도 검토

---

## 마무리

로그 설계는 **어디까지 컴파일에 남길지**, **에디터에서 어떻게 걸러 볼지**, **호출부 비용을 누가 부담하는지**를 함께 정하는 일입니다. P1에서 태그·레벨 필터와 부분적 Conditional의 가치를 확인했고, P2에서는 내부 예외 처리만으로는 호출부 비용이 남는다는 것을 배웠습니다. P3는 컴파일 단계 제거와 에디터 필터를 함께 정리했습니다.

---

## 소스 코드

이 글의 코드 블록은 설계를 이해하기 위한 **발췌**입니다. 세대별 전체 `Log` 클래스는 저장소 [`code/Log/`](https://github.com/IntemStudio/intemstudio.github.io/tree/main/code/Log)에 있습니다.

| 세대 | 파일 | 설명 |
|------|------|------|
| **P1** | [P1_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P1_Log.cs) | `LogSystem`; 일부 출력 API Conditional, `Error` + 파일 로그는 런타임 유지 |
| **P2** | [P2_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P2_Log.cs) | 태그·레벨 필터, `params` 오버로드, Conditional 없음·런타임/`#if` 방어 |
| **P3** (현재) | [P3_Log.cs](https://github.com/IntemStudio/intemstudio.github.io/blob/main/code/Log/P3_Log.cs) | 에디터 전용 출력, Conditional, `Debug.Log` + 색상 |

P3는 `LogTags`, `LogTagFilter`, `GamePrefs` 등 **프로젝트 공통 타입**에 의존합니다. 이 블로그 repo에는 `Log` 클래스 샘플만 포함되어 있어, 그대로 빌드하려면 해당 타입을 프로젝트에 맞게 두어야 합니다.

---

## 참고

- [ConditionalAttribute (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.conditionalattribute)
- [Unity Logging](https://docs.unity3d.com/Packages/com.unity.logging@latest) — 구조화 로그 등 대안
