using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;

public class MainMenuBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject scanningUI; // UI específica para el escaneo
    [SerializeField]
    private Button scanButton; // Botón para escanear QR -> enlazar en el Inspector
    [SerializeField]
    private GameObject canvasMenu; // Canvas del menú inicial
    [SerializeField]
    private GameObject canvasScanner; // Canvas del escaneo de QR
    [SerializeField]
    private Camera cameraScanQr; // Cámara para el escaneo de QR
    [SerializeField]
    private RawImage rawImage; // RawImage para mostrar la vista previa de la cámara de escaneo

    private WebCamTexture webCamTexture;
    private IBarcodeReader barcodeReader = new BarcodeReader(); // Instancia de QR
    private bool isScanning = false;

    void Start()
    {
        // Vincular el método ScanQRCode al evento onClick del botón
        scanButton.onClick.AddListener(ScanQRCode);
        // Asegúrate de que solo el canvas del menú está activo al inicio
        canvasMenu.SetActive(true);
        canvasScanner.SetActive(false);
        cameraScanQr.gameObject.SetActive(false);
    }

    public void ScanQRCode()
    {
        // Mostrar la UI de escaneo y empezar a escanear
        canvasMenu.SetActive(false); // Ocultar el menú
        canvasScanner.SetActive(true); // Mostrar el escáner
        scanningUI.SetActive(true);
        isScanning = true;

        // Activar la cámara de escaneo
        cameraScanQr.gameObject.SetActive(true);

        // Configurar y activar la cámara del dispositivo
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

        // Ajustar la rotación de la cámara
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;

        // Rotar la imagen del RawImage si está volteada
        rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -90);  // Ajusta según sea necesario
    }

    void Update()
    {
        if (!isScanning || webCamTexture == null || !webCamTexture.isPlaying) return;

        try
        {
            var barcodeBitmap = new Texture2D(webCamTexture.width, webCamTexture.height);
            barcodeBitmap.SetPixels(webCamTexture.GetPixels());
            barcodeBitmap.Apply();

            var result = barcodeReader.Decode(barcodeBitmap.GetPixels32(), barcodeBitmap.width, barcodeBitmap.height);
            if (result != null)
            {
                Debug.Log("QR Code detected: " + result.Text);
                // Pasar la información del QR escaneado a la escena principal
                PlayerPrefs.SetString("QrCodeResult", result.Text);
                PlayerPrefs.Save();
                // Cambiar a la escena principal después de detectar el código QR
                isScanning = false; // Detener el escaneo
                webCamTexture.Stop();
                SceneManager.LoadScene("MainScene");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error scanning QR code: " + ex.Message);
        }
    }
}
