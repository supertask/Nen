using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CommandBufferBlurRefraction : MonoBehaviour
{
    public Shader m_BlurShader;
    private Material m_Material;

    private Camera m_Cam;

    // 全てのカメラに対して CommandBUffer を適用するために辞書型
    private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

    // 全ての CommandBUffer をクリア
    private void Cleanup()
    {
        foreach (var cam in m_Cameras)
        {
            if (cam.Key)
            {
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterSkybox, cam.Value);
            }
        }
        m_Cameras.Clear();
        Object.DestroyImmediate(m_Material);
    }

    public void OnEnable()
    {
        Cleanup();
    }

    public void OnDisable()
    {
        Cleanup();
    }

    // OnWillRenderObject() はカメラごとに 1 度ずつ呼び出される
    // ここで Camera.current を全て登録してあげれば全てのカメラに対して処理できる
    public void OnWillRenderObject()
    {
        // --- ここからは初期化 ---

        // 有効でない場合はクリーン
		if (!gameObject.activeInHierarchy || !enabled) {
            Cleanup();
            return;
        }

        // 現在のカメラを取得
        var cam = Camera.current;
        if (!cam) return;

        // 既に CommandBuffer を適用済みなら何もしない
        if (m_Cameras.ContainsKey(cam)) return;

        // マテリアルの初期化
        // m_BlurShader は後述のブラーを適用するシェーダ
        if (!m_Material) {
            m_Material = new Material(m_BlurShader);
            m_Material.hideFlags = HideFlags.HideAndDontSave;
        }

        // --- ここから CommandBuffer の構築 ---

        // CommandBuffer の生成と登録
        var buf = new CommandBuffer();
        buf.name = "Grab screen and blur";
        m_Cameras[cam] = buf;

        // 現在のレンダリング結果をカメラと同じ解像度の一時的な Render Texture へコピー
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        buf.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

        // 半分の解像度で更に 2 枚の Render Texture を生成
        int blurredID = Shader.PropertyToID("_Temp1");
        int blurredID2 = Shader.PropertyToID("_Temp2");
        buf.GetTemporaryRT(blurredID, -2, -2, 0, FilterMode.Bilinear);
        buf.GetTemporaryRT(blurredID2, -2, -2, 0, FilterMode.Bilinear);

        // 半分の解像度へスケールダウンしてコピー
        buf.Blit(screenCopyID, blurredID);

        // フル解像度の Render Target はもう利用しないので解放
        buf.ReleaseTemporaryRT(screenCopyID);

        // 横方向のブラー
        buf.SetGlobalVector("offsets", new Vector4(2.0f/Screen.width, 0, 0, 0));
        buf.Blit(blurredID, blurredID2, m_Material);

        // 縦方向のブラー
        buf.SetGlobalVector("offsets", new Vector4(0, 2.0f/Screen.height, 0, 0));
        buf.Blit(blurredID2, blurredID, m_Material);

        // オフセットを調整して再度横方向のブラー
        buf.SetGlobalVector("offsets", new Vector4(4.0f/Screen.width, 0, 0, 0));
        buf.Blit(blurredID, blurredID2, m_Material);

        // 同じく縦方向のブラー
        buf.SetGlobalVector("offsets", new Vector4(0, 4.0f/Screen.height, 0, 0));
        buf.Blit(blurredID2, blurredID, m_Material);
		/*
        */

        // 結果を _GrabBlurTexture へ格納
        buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

        // --- CommandBuffer の構築完了 ---

        // という一連の流れを記録した CommandBuffer をスカイボックス描画後のタイミングで
        // 適用するように現在のカメラへと登録する
        cam.AddCommandBuffer(CameraEvent.AfterSkybox, buf);
    }
}