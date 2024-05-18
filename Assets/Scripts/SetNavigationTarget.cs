using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown navigationTargetDropDown;
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();
    [SerializeField]
    private float navigationYOffset = 0.0f; // Offset de navegación
    [SerializeField]
    private GameObject spherePrefab; // Prefab de la esfera
    [SerializeField]
    private float sphereSpacing = 1.0f; // Espaciado entre esferas
    [SerializeField]
    private GameObject targetIconPrefab; // Prefab del icono del target
    [SerializeField]
    private int fadeSphereCount = 5; // Número de esferas al final para aplicar fade

    private NavMeshPath path; // Ruta actual calculada
    private List<GameObject> spheres = new List<GameObject>(); // Lista para almacenar las esferas instanciadas
    private Vector3 targetPosition = Vector3.zero; // Posición del objetivo actual
    private bool pathVisible = false;
    private GameObject currentTargetIcon; // Icono del target actual

    private void Start()
    {
        path = new NavMeshPath();
        PopulateDropdown();
    }

    private void Update()
    {
        if (pathVisible && targetPosition != Vector3.zero)
        {
            if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
            {
                ClearSpheres(); // Limpiar esferas anteriores
                Vector3[] calculatedPathAndOffset = AddLineOffset();
                PlaceSpheresAlongPath(calculatedPathAndOffset);
            }
            else
            {
                Debug.Log("Failed to calculate path");
            }
        }
    }

    private Vector3[] AddLineOffset()
    {
        if (navigationYOffset == 0)
        {
            return path.corners;
        }

        Vector3[] calculatedLine = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            calculatedLine[i] = path.corners[i] + new Vector3(0, navigationYOffset, 0);
        }
        return calculatedLine;
    }

    private void PlaceSpheresAlongPath(Vector3[] pathPoints)
    {
        float accumulatedDistance = 0.0f;
        int totalSphereCount = 0;

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector3 startPoint = pathPoints[i];
            Vector3 endPoint = pathPoints[i + 1];
            float segmentDistance = Vector3.Distance(startPoint, endPoint);

            while (accumulatedDistance + sphereSpacing < segmentDistance)
            {
                accumulatedDistance += sphereSpacing;
                Vector3 position = Vector3.Lerp(startPoint, endPoint, accumulatedDistance / segmentDistance);
                position = AdjustPositionForWalls(position);
                if (IsValidPosition(position))
                {
                    GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                    spheres.Add(sphere);
                    totalSphereCount++;
                }
            }
            accumulatedDistance -= segmentDistance;
        }

        // Aplicar fade a las últimas esferas
        ApplyFadeToSpheres(totalSphereCount);
    }

    private Vector3 AdjustPositionForWalls(Vector3 position)
    {
        RaycastHit hit;
        float sphereRadius = 0.5f;
        float adjustDistance = 0.2f;

        // Raycasts en 8 direcciones para ajustar la posición si hay una pared cerca
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                                (Vector3.forward + Vector3.left).normalized, (Vector3.forward + Vector3.right).normalized,
                                (Vector3.back + Vector3.left).normalized, (Vector3.back + Vector3.right).normalized };

        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(position, direction, out hit, sphereRadius))
            {
                position -= direction * adjustDistance;
            }
        }

        return position;
    }

    private void ApplyFadeToSpheres(int totalSphereCount)
    {
        int startFadeIndex = Mathf.Max(0, totalSphereCount - fadeSphereCount);

        for (int i = startFadeIndex; i < totalSphereCount; i++)
        {
            float alpha = Mathf.Lerp(0.1f, 1.0f, (float)(i - startFadeIndex) / fadeSphereCount);
            Color sphereColor = spheres[i].GetComponent<Renderer>().material.color;
            sphereColor.a = alpha;
            spheres[i].GetComponent<Renderer>().material.color = sphereColor;
        }
    }

    private bool IsValidPosition(Vector3 position)
    {
        return float.IsFinite(position.x) && float.IsFinite(position.y) && float.IsFinite(position.z);
    }

    public void SetCurrentNavigationTarget(int selectedValue)
    {
        targetPosition = Vector3.zero;
        string selectedText = navigationTargetDropDown.options[selectedValue].text;
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.Equals(selectedText));
        if (currentTarget != null)
        {
            targetPosition = currentTarget.PositionObject.transform.position;
            Debug.Log("Target set to: " + currentTarget.Name);
            ShowTargetIcon(currentTarget);
        }
        else
        {
            Debug.Log("No target found for: " + selectedText);
        }
    }

    private void ShowTargetIcon(Target target)
    {
        if (currentTargetIcon != null)
        {
            Destroy(currentTargetIcon);
        }

        // Instanciar el icono del target
        currentTargetIcon = Instantiate(targetIconPrefab, target.PositionObject.transform.position, Quaternion.identity);
        currentTargetIcon.transform.SetParent(target.PositionObject.transform); // Hacer que el icono sea hijo del target

        // Asignar el nombre del target al componente de texto
        TextMeshProUGUI textMeshPro = currentTargetIcon.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = target.Name;
        }
    }

    public void ToggleVisibility()
    {
        pathVisible = !pathVisible;
        if (!pathVisible)
        {
            ClearSpheres();
        }
        else if (targetPosition != Vector3.zero)
        {
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            UpdatePath();
        }
        Debug.Log("Path visibility toggled: " + pathVisible);
    }

    private void ClearSpheres()
    {
        foreach (GameObject sphere in spheres)
        {
            Destroy(sphere);
        }
        spheres.Clear();
    }

    private void UpdatePath()
    {
        ClearSpheres();
        if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
        {
            Vector3[] calculatedPathAndOffset = AddLineOffset();
            PlaceSpheresAlongPath(calculatedPathAndOffset);
        }
    }

    private void PopulateDropdown()
    {
        navigationTargetDropDown.options.Clear();
        foreach (var target in navigationTargetObjects)
        {
            navigationTargetDropDown.options.Add(new TMP_Dropdown.OptionData(target.Name));
        }
        navigationTargetDropDown.RefreshShownValue();
    }
}
