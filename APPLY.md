# 적용 방법

이 폴더의 `README.md`와 `Docs/` 디렉터리를
`ashborn-client-portfolio` 저장소 루트에 그대로 복사하면 됩니다.

이번 완성본에는 다음 파일이 모두 포함되어 있습니다.

- 최적화·트러블슈팅이 반영된 루트 `README.md`
- 상세 기술 문서 `Docs/technical/README.md`
- 시스템 아키텍처 문서와 Mermaid 원본
- 게임 원본 캡처 11장
- 기능 흐름 합성 이미지 3장
- 이미지 파일 목록 문서

## 이미지 경로

```text
Docs/screenshots/portfolio/
├─ original/    # 게임에서 직접 캡처한 원본 이미지
├─ composite/   # README용 기능 흐름 합성 이미지
└─ README.md    # 이미지 목록
```

## PowerShell 적용 예시

```powershell
cd <ashborn-client-portfolio 경로>

git switch -c docs/portfolio-performance-troubleshooting

Copy-Item "<완성본 경로>\README.md" ".\README.md" -Force
Copy-Item "<완성본 경로>\Docs" ".\Docs" -Recurse -Force

git add README.md Docs
git commit -m "docs: add optimization troubleshooting and original screenshots"
git push -u origin docs/portfolio-performance-troubleshooting
```

## 변경 사항

- 프로젝트 개요에서 `담당 범위` 항목 제거
- AI 활용 범위를 의사결정·기획 보조로 명시
- 전반적인 코드 개발은 AI 도움을 거의 받지 않고 직접 수행했다고 명시
- README가 실제 포함된 이미지 경로만 참조하도록 정리
- 원본 이미지와 합성 이미지를 별도 폴더로 분리
