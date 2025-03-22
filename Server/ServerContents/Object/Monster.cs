using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    class Monster : GameObject
    {
        public MonsterInfo Info { get; set; } = new MonsterInfo();
        public MonsterStatInfo Stat { get; set; } = new MonsterStatInfo();

        protected Player? _target = null;

        protected Random rnd = new Random();

        protected Vector2 _destinationPos;
        protected bool _isRight = false;

        // 이동 가능한 범위
        protected float _minX;
        protected float _maxX;

        // 랜덤한 Action을 선택하는 Think 함수에서 사용될 변수
        protected DateTime _lastThinkTime;
        protected float _thinkInterval;

        // Stun
        protected DateTime _stunStartTime;
        protected float _stunDuration = 2.0f;

        // Skill
        protected DateTime _skillStartTime;
        protected float _skillDuration = 3.5f;

        public Monster() { Info.StatInfo = Stat; }


        //======================================================================================================
        // 자식클래스의 구조에 따라 재정의가 필요한 함수.
        //====================================================================================================== 

        public virtual void SetTarget(Player newTarget) { }

        public virtual void TakeDamage(int playerId, List<int> damageAmounts)
        {
            int totalDamageAmount = 0;
            foreach (var damageAmount in damageAmounts)
                totalDamageAmount += damageAmount;
            Stat.Hp -= totalDamageAmount; 
        }

        protected virtual void BroadcastMove() { }

        protected virtual void BroadcastSkill() { }

        protected virtual void UpdateInfo() { }

        protected virtual void UpdateThink() { }

        protected virtual void Think() { }

        protected virtual void UpdateIdle() { }

        protected virtual void UpdateMove() { }

        protected virtual void UpdateStun() { }

        protected virtual void UpdateSkill() { }

        protected virtual void UpdateDead() { }


        //======================================================================================================
        // 자식클래스에서 구조 변경이 필요없음. 공통적으로 사용되는 함수. 재정의 필요없음
        //====================================================================================================== 

        // 스폰 포지션
        public void SetInitialPos(Vector2 spawnPos) => (_destinationPos.X, _destinationPos.Y) = (spawnPos.X, spawnPos.Y);

        // 이동 가능한 범위
        public void SetBoundPos(Vector2 boundPos) => (_minX, _maxX) = (boundPos.X, boundPos.Y);

        // 타겟으로 지정된 플레이어가 맵에서 사라진 경우, 타겟을 null로 변화시키기 위해 Update에서 지속적으로 호출되는 함수
        protected void UpdateTarget() 
        {
            if (_target != null && Room.IsPlayerInRoom(_target.Id) == false)
            {
                _target = null;
                return;
            }
        }
    }
}
