# AGENTS.md

Intem Studio **개발 블로그** 저장소(`intemstudio.github.io`)에서 코딩 에이전트가 따를 지침입니다.  
사람용 소개는 루트 `index.md`를 참고하세요.

## 프로젝트 개요

- **스택:** Jekyll + minima 테마, GitHub Pages 배포
- **URL:** https://intemstudio.github.io
- **콘텐츠:** Unity 엔지니어링 글(`unity/`), 게임 개발 노트(`games/`), 참고 코드(`code/`)
- **내부 문서:** `docs/` — Jekyll `exclude`로 **사이트에 게시되지 않음**

## 디렉터리 구조

```
├── unity/              # Unity 엔지니어링 페이지 (permalink, front matter)
├── games/              # 게임 개발 노트
├── code/               # 글에서 인용하는 샘플 코드 (예: code/Log/)
├── _posts/             # 날짜 기반 포스트 (categories: unity 등)
├── docs/               # 작성 가이드·프로젝트 규칙 (내부용, 미게시)
├── .cursor/rules/      # Cursor 전용 규칙 (.mdc)
└── _config.yml         # Jekyll 설정
```

- `unity/index.md`는 `unity/` 하위 페이지와 `_posts`의 `categories: unity` 글을 목록에 표시합니다.
- 블로그 글은 `unity/*.md` 페이지 또는 `_posts/*.md` 포스트로 추가합니다.

## 빌드·검증

Ruby + Bundler가 있을 때:

```bash
bundle install
bundle exec jekyll build
bundle exec jekyll serve   # 로컬 미리보기 (http://127.0.0.1:4000)
```

작업 후 가능하면 `jekyll build`로 빌드 오류가 없는지 확인합니다.  
새 페이지·포스트는 `permalink`와 내부 앵커 링크가 배포 환경에서 동작하는지 확인합니다.

## 작업 시 규칙

### 커밋

- **제목·본문 모두 한글**로 작성합니다.
- 사이트 콘텐츠와 내부 문서·설정은 **커밋을 나눕니다**.
- 사용자가 명시적으로 요청하기 전에는 **커밋·push하지 않습니다**.

상세: [docs/project-rules.md](docs/project-rules.md)

### 블로그 글 작성

- 공통 규칙: [docs/writing-guide.md](docs/writing-guide.md)
- 시스템 진화형 설계 글: [docs/writing/templates/system-evolution.md](docs/writing/templates/system-evolution.md)
- 대표 사례: [unity/editor-log.md](unity/editor-log.md)

front matter 예시 (`unity/` 페이지):

```yaml
---
layout: page
title: "기술 요소 — 맥락"
permalink: /unity/슬러그/
tags:
  - unity
  - logging
---
```

### 하지 말 것

- `.vscode/` 등 로컬 IDE 설정을 커밋하지 않습니다.
- `docs/` 내용을 사이트에 올리려고 Jekyll exclude를 제거하지 않습니다 (의도된 내부 문서).
- 비밀·자격 증명 파일을 커밋하지 않습니다.
- 요청 없이 관련 없는 리팩터링·문서 파일을 추가하지 않습니다.

## Cursor 규칙

`.cursor/rules/`의 `.mdc` 파일이 Cursor에서 추가로 적용됩니다.

- `commit-messages-korean.mdc` — 커밋 메시지 한글 (`alwaysApply`)

상세 규칙의 **본문**은 `docs/`에 두고, AGENTS.md는 진입점·링크만 유지합니다. 규칙이 늘면 `docs/project-rules.md`에 추가하고 여기서 링크를 갱신합니다.

## 외부 링크

- GitHub: https://github.com/IntemStudio/intemstudio.github.io
- 글 속 코드 링크는 `IntemStudio/intemstudio.github.io` blob/tree URL을 사용합니다.
