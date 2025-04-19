# 🍁 [Unity 2D] MapleLike

<div align="center">
  <a href="https://www.youtube.com/watch?v=XNInQwFxtAw">
    <img src="https://img.youtube.com/vi/XNInQwFxtAw/0.jpg" alt="[Unity 2D] MapleLike">
  </a>   
</div>

<br>

## 🎮 프로젝트 개요

- **기간**: 2024.12 ~ 2025.03
- **목표**:
    - Unity 엔진을 활용한 MMORPG 게임 *메이플스토리* 모작 개발
    - 멀티플레이 게임 개발 및 네트워크 구조에 대한 이해
    - 서버-클라이언트 간 협업 및 실전 개발 경험 습득

> 📄 [프로젝트 계획서](https://github.com/study-kim7507/Unity2D_MapleLike/blob/main/plan.pdf)  
> 📄 [프로젝트 소개 (개인 포트폴리오)](https://github.com/study-kim7507/Unity2D_MapleLike/blob/main/introduction.pdf)

<br>

## 👥 팀 구성 및 역할

| 이름       | 역할         | 담당 업무                                                                 |
|------------|--------------|--------------------------------------------------------------------------|
| **임민혁** | 팀장 / 서버  | - 기획<br>- 서버 개발 및 유지보수                                         |
| **김기환** | 클라이언트   | - 서버 (몬스터, 플레이어 씬 전환)<br>- 클라이언트 (몬스터, 맵 디자인, 퀘스트 시스템) |
| **이현승** | 클라이언트   | - 캐릭터 시스템 구조 및 스탯 시스템<br>- 캐릭터 관련 UI                  |
| **김기태** | 클라이언트   | - 캐릭터 스킬 및 애니메이션 구현                                          |
| **송경원** | 클라이언트   | - 아이템/인벤토리 시스템<br>- 전반적인 UI 구성                            |

<br>

## 🔧 개발 환경

| 분류               | 사용 툴                                                                                                                                                                                                                       |
|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **언어 & 엔진**     | <img src="https://img.shields.io/badge/C%23-00599C?style=for-the-badge&logo=c%2B%2B&logoColor=white"> <img src="https://img.shields.io/badge/unity Engine-FFFFFF.svg?style=for-the-badge&logo=unity&logoColor=black">       |
| **협업 툴**         | <img src="https://img.shields.io/badge/discord-5865F2.svg?style=for-the-badge&logo=discord&logoColor=white"> <img src="https://img.shields.io/badge/notion-white.svg?style=for-the-badge&logo=notion&logoColor=black">   |
| **버전 & 이슈 관리** | <img src="https://img.shields.io/badge/github-181717.svg?style=for-the-badge&logo=github&logoColor=white"> <img src="https://img.shields.io/badge/plasticscm-47a3ea.svg?style=for-the-badge">                               |

<br>

## ⏩ 프로젝트 실행 방법 (싱글플레이 - 로컬 서버)

> ⚠️ **현재 AWS 서버는 닫혀 있으며, 로컬 서버 기반의 싱글플레이만 지원됩니다.**

1. 레포지토리 내 `Build` 폴더와 `Server` 폴더를 다운로드합니다.
2. `Server/ServerContents/bin/Debug/net8.0/ServerContents.exe`를 실행합니다.
3. `Build/MapleLike.exe`를 실행하여 게임을 시작합니다.


