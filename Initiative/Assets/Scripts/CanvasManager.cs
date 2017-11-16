using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : Singleton<CanvasManager> {

    [SerializeField, Range(0, 100)] private int StartNumberOfPlayers = 4;
    [SerializeField, Range(0, 1), Tooltip("The percent of the screen width that the swipe length should be")]
    private float SwipeLengthPercent = 0.15f;
    private float CalculatedSwipeLength;
    [SerializeField] private float Epsilon = 0.1f;
    // The amount of pixels in altitude variance while flying
    [SerializeField] private float FlyHeightVariance = 400f;
    [SerializeField] private float FlightSpeedMin = 100f;
    [SerializeField] private float FlightSpeedMax = 500f;
    [SerializeField] private float CrowScaleMin = 0.2f;
    [SerializeField] private float CrowScaleMax = 0.5f;
    [SerializeField] private int FlyChance = 4;
    [SerializeField] private float TouchHoldToolTipDisplaySeconds = 0.5f;
    // Crow Animation
    [SerializeField] private RectTransform FlyZone;
    [SerializeField] private GameObject[] crows;
    // internal variables
    private float FieldXPosition;
    private PlayerInputField SelectedPlayerField = null;
    private RectTransform PotentialDeleteTransform;
    private Vector2 TouchStartPosition;
    private float SecondsTimer = 0;
    private float TouchHoldTime = 0;

    // External References
    [SerializeField] private InitiativeCalculator Calculator;
    [SerializeField] private RectTransform Content;
    [SerializeField] private Transform VerticalLayout;
    [SerializeField] private GameObject PlayerInputPrefab;
    [SerializeField] private Transform AddPlayerButton;
    [SerializeField] private RectTransform DeleteDialogue;
    [SerializeField] private Animator TooltipAnimator;

    void Start ()
    {
        // Find out how many player input fields to add by ignoring the existing ones and the "add" button
        int players_to_add = StartNumberOfPlayers - (VerticalLayout.childCount - 1);
        for (int i = 0; i < players_to_add; ++i)
        {
            AddPlayer();
        }
        CalculatedSwipeLength = SwipeLengthPercent * Screen.width;
        CloseDeleteDialogue();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ResetContent();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Get a reference to the PlayerInputField that was tapped
            PlayerInputField[] player_fields = VerticalLayout.GetComponentsInChildren<PlayerInputField>();
            foreach (PlayerInputField field in player_fields)
            {
                if (field.IsTapped(Input.GetTouch(0).position))
                {
                    // If null fields, then it is the AddPlayerButton
                    if (!field.Name)
                    {
                        AddPlayer();
                        AudioManager.Instance.PlayAudio("Pop");
                    }
                    else
                    {
                        SelectedPlayerField = field;
                        TouchStartPosition = Input.GetTouch(0).position;
                        FieldXPosition = field.transform.position.x;
                    }
                    break;
                }
            }
        }
        else if (Input.touchCount > 0 && SelectedPlayerField && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Ask to delete if swiped left
            if (Input.GetTouch(0).position.x < TouchStartPosition.x)
            {
                float touch_delta_x = TouchStartPosition.x - Input.GetTouch(0).position.x;
                // If swiped left long enough to delete
                if (touch_delta_x >= CalculatedSwipeLength)
                {
                    // Show the delete dialogue next to the selected player input field
                    DeleteDialogue.gameObject.SetActive(true);
                    PotentialDeleteTransform = SelectedPlayerField.GetComponent<RectTransform>();
                }
                else
                {
                    // Shift the field left with the finger
                    float y_position = SelectedPlayerField.GetComponent<RectTransform>().position.y;
                    float x_position = FieldXPosition - touch_delta_x;
                    SelectedPlayerField.transform.position =
                        new Vector2(x_position, y_position);
                }
            }
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            TouchHoldTime = 0;
            // Check if touch ended on the Field and touch is close to start touch position
            if (SelectedPlayerField.NameTapped(Input.GetTouch(0).position) && 
                Vector2.Distance(Input.GetTouch(0).position, TouchStartPosition) < 0.1 * Screen.width)
            {
                SelectedPlayerField.Name.interactable = true;
                SelectedPlayerField.Name.Select();
                SelectedPlayerField.Name.ActivateInputField();
                SelectedPlayerField.Name.interactable = false;
            }
            else if (SelectedPlayerField.ModifierTapped(Input.GetTouch(0).position) &&
                Vector2.Distance(Input.GetTouch(0).position, TouchStartPosition) < 0.1 * Screen.width)
            {
                SelectedPlayerField.Modifier.interactable = true;
                SelectedPlayerField.Modifier.Select();
                SelectedPlayerField.Modifier.ActivateInputField();
                SelectedPlayerField.Modifier.interactable = false;
            }

            SelectedPlayerField = null;
            // Reset the vertical layout group positions
            AddPlayerButton.SetAsLastSibling();
        }
        // Count hold time for tooltip
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary &&
            SelectedPlayerField.IsTapped(Input.GetTouch(0).position))
        {
            TouchHoldTime += Time.deltaTime;
        }

        // Check to display Tooltip
        if (TouchHoldTime >= TouchHoldToolTipDisplaySeconds &&
            !TooltipAnimator.GetCurrentAnimatorStateInfo(0).IsName("TooltipAnimation"))
        {
            TooltipAnimator.SetTrigger("Display");
            TouchHoldTime = 0;
        }

        // The DeleteDialogue follows the selected field
        if (DeleteDialogue.gameObject.activeSelf)
        {
            DeleteDialogue.position = PotentialDeleteTransform.position;
        }

        // Animate Crows, for a 1 out of crow_fly_chance every second
        if (SecondsTimer <= 0)
        {
            SecondsTimer = 1f;
            int crow_fly_chance = Random.Range(0, FlyChance);
            if (crow_fly_chance == 0)
            {
                int crow_index = Random.Range(0, crows.Length);
                CrowFly(Instantiate(crows[crow_index], FlyZone).GetComponent<RectTransform>());
            }
        }
        else
        {
            SecondsTimer -= Time.deltaTime;
        }
    }
    
    public List<Player> GetData()
    {
        List<Player> players = new List<Player>();
        PlayerInputField[] player_fields = VerticalLayout.GetComponentsInChildren<PlayerInputField>();
        foreach (PlayerInputField field in player_fields)
        {
            if (field.Name && field.Name.text != "")
            {
                // Get modifier
                int modifier = 0;
                try
                {
                    modifier = int.Parse(field.Modifier.text);
                }
                catch
                {
                    modifier = 0;
                }
                // Roll for Initiative
                int initiative = Calculator.GetRandRoll() + modifier;
                // Create Player data object
                players.Add(new Player(field.Name.text, modifier, initiative));
            }
        }
        return players;
    }

    public void AddPlayer()
    {
        Transform new_player_input = Instantiate(PlayerInputPrefab, VerticalLayout).transform;
        new_player_input.SetAsLastSibling();
        AddPlayerButton.SetAsLastSibling();
        UpdateContentSize();
    }

    public void ResetContent()
    {
        if (Content.localPosition.y > 1 || Content.localPosition.y < -1)
        {
            Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, 0);
        }
    }

    private void UpdateContentSize()
    {
        float new_height = PlayerInputPrefab.GetComponent<RectTransform>().sizeDelta.y * VerticalLayout.childCount;
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, new_height);
    }

    public void CloseDeleteDialogue()
    {
        DeleteDialogue.gameObject.SetActive(false);
    }

    public void DeletePlayerField()
    {
        if (SelectedPlayerField)
        {
            DestroyImmediate(SelectedPlayerField.gameObject);
            SelectedPlayerField = null;
        }
        CloseDeleteDialogue();
    }

    private void CrowFly(RectTransform crow)
    {
        float y_pos = Random.Range(0, FlyZone.sizeDelta.y);
        float start_x, end_x;
        if (crow.name == "RightCrow(Clone)")
        {
            start_x = 0;
            end_x = FlyZone.sizeDelta.x;
        }
        else
        {
            start_x = FlyZone.sizeDelta.x;
            end_x = 0;
        }
        Vector2 goal = new Vector2(end_x, y_pos + Random.Range(-FlyHeightVariance / 2, FlyHeightVariance / 2));
        crow.localPosition = new Vector2(start_x, y_pos);
        float speed = Random.Range(FlightSpeedMin, FlightSpeedMax);
        // Scale the crow according to the ration of its speed to simulate distance
        float crow_scale = (speed - FlightSpeedMin) / (FlightSpeedMax - FlightSpeedMin);
        crow_scale = (crow_scale * (CrowScaleMax - CrowScaleMin)) + CrowScaleMin;
        crow.localScale = new Vector3(crow_scale, crow_scale, 1);
        StartCoroutine(MoveAndDestroy(crow, goal, speed));
    }

    private IEnumerator MoveAndDestroy(RectTransform mover, Vector2 goal, float Speed)
    {
        while (Vector2.Distance(mover.localPosition, goal) > Epsilon)
        {
            mover.localPosition = Vector2.MoveTowards(mover.localPosition, goal, Time.deltaTime * Speed);
            yield return null;
        }
        DestroyImmediate(mover.gameObject);
    }

}
