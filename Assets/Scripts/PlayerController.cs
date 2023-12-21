using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterControllerPrototype networkCharacterController = null;

    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private float moveSpeed = 15f;

    [Networked]
    public NetworkButtons ButtonsPrevious { get; set; }

    [SerializeField]
    private Image hpBar = null;

    [SerializeField]
    private int maxHp = 100;

    [Networked(OnChanged = nameof(OnHpChanged))]
    public int Hp { get; set; }

    [SerializeField]
    private MeshRenderer meshRenderer = null;

    [SerializeField]
    private Outline outline;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeColor_RPC(Color.red);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeColor_RPC(Color.green);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeColor_RPC(Color.blue);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeColor_RPC(Color.yellow);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ChangeColor_RPC(Color.magenta);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ChangeColor_RPC(Color.cyan);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ChangeColor_RPC(Color.black);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void ChangeColor_RPC(Color newColor)
    {
        meshRenderer.material.color = newColor;
        outline.OutlineColor = newColor;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            NetworkButtons buttons = data.buttons;
            var pressed = buttons.GetPressed(ButtonsPrevious);
            ButtonsPrevious = buttons;

            Vector3 moveVector = data.movementInput.normalized;
            networkCharacterController.Move(moveSpeed * moveVector * Runner.DeltaTime);

            if (pressed.IsSet(InputButtons.JUMP))
            {
                networkCharacterController.Jump();
            }

            if (pressed.IsSet(InputButtons.FIRE))
            {
                Runner.Spawn(
                    bulletPrefab,
                    transform.position + transform.TransformDirection(Vector3.forward),
                    Quaternion.LookRotation(transform.TransformDirection(Vector3.forward)),
                    Object.InputAuthority);
            }
        }

        if (Hp <= 0 || networkCharacterController.transform.position.y <= -5f)
        {
            Respawn();
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Hp = maxHp;
    }

    private void Respawn()
    {
        networkCharacterController.transform.position = Vector3.up * 4;
        Hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (Object.HasStateAuthority)
        {
            Hp -= damage;
        }
    }

    private static void OnHpChanged(Changed<PlayerController> changed)
    {
        changed.Behaviour.hpBar.fillAmount = (float)changed.Behaviour.Hp / changed.Behaviour.maxHp;
    }
}
