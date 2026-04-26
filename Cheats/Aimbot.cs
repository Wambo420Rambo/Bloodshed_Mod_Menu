using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        // ── Aimbot fields ─────────────────────────────────────────────────
        private bool _aimbotEnabled;
        private float _circleRadius = 150f;
        private float _aimSmoothing = 10f;
        private float _aimTargetOffset = 1.5f;
        private GameObject _currentTarget;
        private Texture2D _circleTex;
        private const float SmoothingMin = 1f;
        private const float SmoothingMax = 99f;

        // ── Called from Update ────────────────────────────────────────────

        private void UpdateAimbot()
        {
            if (!_aimbotEnabled)
            {
                _currentTarget = null;
                return;
            }

            _currentTarget = GetClosestEnemyInCircle(_cachedEnemies);
            if (_currentTarget != null)
                AimAtTarget(_currentTarget.transform);
        }

        // ── Called from OnGUI ─────────────────────────────────────────────

        private void DrawAimbotCircle()
        {
            if (_circleTex == null)
                _circleTex = CreateCircleTexture(200, Color.red, 3f);

            float diameter = _circleRadius * 2f;
            Vector2 center = new(Screen.width / 2f, Screen.height / 2f);

            GUI.color = _currentTarget != null
                ? new Color(1f, 0.2f, 0.2f, 0.9f)
                : new Color(1f, 1f, 1f, 0.45f);

            GUI.DrawTexture(
                new Rect(center.x - _circleRadius, center.y - _circleRadius, diameter, diameter),
                _circleTex);

            GUI.color = Color.white;
        }

        // ── Target selection ──────────────────────────────────────────────

        private GameObject GetClosestEnemyInCircle(List<GameObject> enemies)
        {
            if (_cam == null) return null;

            Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);
            Vector3 myPos = _cam.transform.position;
            GameObject closest = null;
            float closestWorld = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                Vector3 screenPos = _cam.WorldToScreenPoint(enemy.transform.position);

                // Behind camera or off-screen
                if (screenPos.z <= 0f) continue;
                if (screenPos.x < 0f || screenPos.x > Screen.width ||
                    screenPos.y < 0f || screenPos.y > Screen.height) continue;

                // Outside aim circle
                if (Vector2.Distance(screenCenter, new Vector2(screenPos.x, screenPos.y)) > _circleRadius)
                    continue;

                // Visibility raycast – aim point matches AimAtTarget
                Vector3 aimPoint = enemy.transform.position + Vector3.up * _aimTargetOffset;
                Vector3 direction = aimPoint - myPos;

                if (Physics.Raycast(myPos, direction, out RaycastHit hit, direction.magnitude))
                    if (!hit.collider.gameObject.CompareTag("Enemy")) continue;

                float worldDist = Vector3.Distance(myPos, enemy.transform.position);
                if (worldDist >= closestWorld) continue;

                closestWorld = worldDist;
                closest = enemy;
            }

            return closest;
        }

        private void AimAtTarget(Transform target)
        {
            if (_cam == null || _controller == null) return;

            Vector3 aimPoint = target.position + Vector3.up * _aimTargetOffset;
            Quaternion targetRot = Quaternion.LookRotation(aimPoint - _cam.transform.position);

            float yaw = targetRot.eulerAngles.y;
            float pitch = targetRot.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            float t = Time.deltaTime * _aimSmoothing;

            _controller.mouseLook.m_CharacterTargetRot = Quaternion.Slerp(
                _controller.mouseLook.m_CharacterTargetRot,
                Quaternion.Euler(0f, yaw, 0f), t);

            _controller.mouseLook.m_CameraTargetRot = Quaternion.Slerp(
                _controller.mouseLook.m_CameraTargetRot,
                Quaternion.Euler(pitch, 0f, 0f), t);
        }
    }
}
