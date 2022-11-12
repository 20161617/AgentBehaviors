using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentBehavior : MonoBehaviour
{
    public enum BEHAVIORS
    {
        SEEK,
        FLEE,
        ARRIVE,
        PURSUIT,
        EVADE,
        WANDER,
        OFFSETPURSUIT,
        INTERPOSE,
        HIDE
    }
    private static AgentBehavior instance = null;

    private StateMachine stateMachine;
    private IState iState;

    private Dictionary<BEHAVIORS, IState> dictionaryState = new Dictionary<BEHAVIORS, IState>();

    public BEHAVIORS behaviors;

    public GameObject Agent;
    public GameObject target;
    public GameObject leader;

    public GameObject interPoseAgentX;
    public GameObject interPoseAgentY;

    public float maxSpeed = 3.0f;
    public float m_dWanderJitter;

    public Vector2 m_vWanderTarget;
    public float m_dWanderRadius;

    public float theta;
    //const float TwoPi = Pi * 2.0f;
    //const float Pi = 3.14159f;
    public float m_dWanderDistance;

    public Vector2 velocity;
    public Vector2 agentPos;

    public Vector2 steering;

    public Vector2 desiredVelocity;

    public float angle;
    //
    //public Rigidbody rigidbody;

    public Vector2 offsetPosition;

    public GameObject Hunter;
    public List<GameObject> circles = new List<GameObject>();
    // Start is called before the first frame update

    private void Awake()
    {
        if(null == instance)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static AgentBehavior Instance
    {
        get
        {
            if(null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    void Start()
    {
        m_dWanderRadius = 5.2f;
        m_dWanderDistance = 2.0f;
        m_dWanderJitter = 80.0f;
        theta = 3.14159f * 2.0f * Random.Range(0.0f, 1.0f);
        agentPos = Agent.transform.position;
        velocity = Vector2.zero;
        m_vWanderTarget = new Vector2(m_dWanderRadius * Mathf.Cos(theta), m_dWanderRadius * Mathf.Sin(theta));
        //

        IState seek = new StateSeek();
        IState flee = new StateFlee();
        IState arrive = new StateArrive();
        IState pursuit = new StatePursuit();
        IState evade = new StateEvade();
        IState wander = new StateWander();
        IState offsetpursuit = new StateOffsetPursuit();
        IState interpose = new StateInterpose();
        IState hide = new StateHide();

        dictionaryState.Add(BEHAVIORS.SEEK, seek);
        dictionaryState.Add(BEHAVIORS.FLEE, flee);
        dictionaryState.Add(BEHAVIORS. ARRIVE, arrive);
        dictionaryState.Add(BEHAVIORS.PURSUIT, pursuit);
        dictionaryState.Add(BEHAVIORS.EVADE, evade);
        dictionaryState.Add(BEHAVIORS.WANDER, wander);
        dictionaryState.Add(BEHAVIORS.OFFSETPURSUIT, offsetpursuit);
        dictionaryState.Add(BEHAVIORS.INTERPOSE, interpose);
        dictionaryState.Add(BEHAVIORS.HIDE, hide);

        stateMachine = new StateMachine(seek);
    }

    // Update is called once per frame
    void Update()
    {
      
        Vector3 screenSize = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenSize.x < 0.0f) screenSize = new Vector3(1.0f, screenSize.y, 10.0f);
        if (screenSize.y < 0.0f) screenSize = new Vector3(screenSize.x, 1.0f, 10.0f);
        if (screenSize.x > 1.0f) screenSize = new Vector3(0.0f, screenSize.y, 10.0f);
        if (screenSize.y > 1.0f) screenSize = new Vector3(screenSize.x, 0.0f, 10.0f);
        this.transform.position = Camera.main.ViewportToWorldPoint(screenSize);

        Vector2 moveLength = velocity * Time.deltaTime;
        transform.Translate(moveLength, Space.World);

      
        if (Input.GetKeyDown(KeyCode.S))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.SEEK]);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.FLEE]);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.ARRIVE]);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.PURSUIT]);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.EVADE]);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.WANDER]);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.OFFSETPURSUIT]);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.INTERPOSE]);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            stateMachine.SetState(dictionaryState[BEHAVIORS.HIDE]);
        }

        stateMachine.DoOperateUpdate();
     
        steering = Vector2.ClampMagnitude(steering, 1);
        steering /= 12.5f;
        velocity = Vector2.ClampMagnitude(velocity + steering, 5);
        
        if (velocity.magnitude > 0.1f)
            RotateToTarget();
       


    }

    void RotateToTarget()
    {
        angle = (Mathf.Atan2(desiredVelocity.y, desiredVelocity.x) * Mathf.Rad2Deg) - 90;

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * maxSpeed * 2);
    }

  
}

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public StateMachine(IState defaultState)
    {
        CurrentState = defaultState;
    }

    public void SetState(IState state)
    {
        if (CurrentState == state)
        {
            Debug.Log("not change");
            return;
        }

      
        CurrentState.OperateExit();
      
        CurrentState = state;
       
        CurrentState.OperateEnter();
       
    }
    public void DoOperateUpdate()
    {
        
        CurrentState.OperateUpdate();
    }
}

public interface IState
{
    void OperateEnter();
    void OperateUpdate();
    void OperateExit();
}

public class StateSeek : MonoBehaviour, IState
{
    public static StateSeek instance;
   
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);

        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
     }

    public static StateSeek Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateSeek)FindObjectOfType(typeof(StateSeek));
                if (instance == null)
                {

                    Debug.Log("no singleton obj");
                }
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Seek(AgentBehavior.Instance.target.transform.position);
        Debug.Log("Seek");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(false);
    }
    public void log()
    {
        Debug.Log("111111111111111111111111111111");
    }
    public Vector2 Seek(Vector2 targetPos)
    {
        AgentBehavior.Instance.desiredVelocity = targetPos - (Vector2)AgentBehavior.Instance.Agent.transform.position;

        AgentBehavior.Instance.desiredVelocity = AgentBehavior.Instance.desiredVelocity.normalized * AgentBehavior.Instance.maxSpeed;

        return (AgentBehavior.Instance.desiredVelocity - AgentBehavior.Instance.velocity);
    }
}
public class StateFlee : MonoBehaviour, IState
{
    public static StateFlee instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateFlee Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateFlee)FindObjectOfType(typeof(StateFlee)) as StateFlee;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Flee(AgentBehavior.Instance.target.transform.position);
        Debug.Log("Flee");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(false);
    }
    public Vector2 Flee(Vector2 targetPos)
    {
        AgentBehavior.Instance.desiredVelocity = (Vector2)AgentBehavior.Instance.Agent.transform.position - targetPos;

        AgentBehavior.Instance.desiredVelocity = AgentBehavior.Instance.desiredVelocity.normalized * AgentBehavior.Instance.maxSpeed;

        if (Mathf.Pow(AgentBehavior.Instance.Agent.transform.position.x - targetPos.x, 2) + Mathf.Pow(AgentBehavior.Instance.Agent.transform.position.y - targetPos.y, 2) > 10.0f)
        {
            return AgentBehavior.Instance.velocity = Vector2.zero;
        }

        return (AgentBehavior.Instance.desiredVelocity - AgentBehavior.Instance.velocity);
    }
}
public class StateArrive : MonoBehaviour, IState
{
    public static StateArrive instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateArrive Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateArrive)FindObjectOfType(typeof(StateArrive)) as StateArrive;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Arrive(AgentBehavior.Instance.target.transform.position);
        Debug.Log("Arrive");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(false);
    }
    public Vector2 Arrive(Vector2 targetPos)
    {
        Vector2 ToTarget = targetPos - (Vector2)AgentBehavior.Instance.Agent.transform.position;

        float dist = ToTarget.magnitude;

        float speed = dist / 0.3f;

        if (ToTarget != Vector2.zero)
        {
            AgentBehavior.Instance.desiredVelocity = ToTarget * speed / dist;

            return (AgentBehavior.Instance.desiredVelocity - AgentBehavior.Instance.velocity);
        }

        return Vector2.zero;
    }
}
public class StatePursuit : MonoBehaviour, IState
{
    public static StatePursuit instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StatePursuit Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(StatePursuit)) as StatePursuit;

                if (instance == null)
                {
                    Debug.Log("no singleton obj");
                   
                }
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Pursuer").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Pursuit(AgentBehavior.Instance.target);
        Debug.Log("Pursuit");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Pursuer").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Target").gameObject.SetActive(false);
    }
    public Vector2 Pursuit(GameObject evader)
    {
        StateSeek.Instance.log();
        Vector2 toEvader = (Vector2)evader.transform.position - AgentBehavior.Instance.agentPos;
        
        float relativeHeading = Vector2.Dot(AgentBehavior.Instance.agentPos.normalized, evader.transform.position.normalized);

        if (Vector2.Dot(toEvader, AgentBehavior.Instance.agentPos.normalized) > 0.0f && relativeHeading < -0.95f)
        {
            return StateSeek.Instance.Seek(evader.transform.position);
        }
        float LookAheadTime = toEvader.magnitude / (AgentBehavior.Instance.maxSpeed + evader.transform.position.magnitude / 5.0f);
        return StateSeek.Instance.Seek((Vector2)evader.transform.position + (Vector2)evader.GetComponent<Rigidbody>().velocity * LookAheadTime);
    }
    

}
public class StateEvade : MonoBehaviour, IState
{
    public static StateEvade instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateEvade Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateEvade)FindObjectOfType(typeof(StateEvade)) as StateEvade;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Evader").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Evade(AgentBehavior.Instance.target);
        Debug.Log("Evade");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Evader").gameObject.SetActive(false);
    }
    public Vector2 Evade(GameObject pursuer)
    {

        Vector2 toPursuer = (Vector2)pursuer.transform.position - (Vector2)AgentBehavior.Instance.transform.position;

        float LookAheadTime = toPursuer.magnitude / (AgentBehavior.Instance.maxSpeed + pursuer.transform.position.magnitude / 5.0f);

        return StateFlee.Instance.Flee((Vector2)pursuer.transform.position + (Vector2)pursuer.GetComponent<Rigidbody>().velocity * LookAheadTime);


    }
}
public class StateWander : MonoBehaviour, IState
{
    public static StateWander instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateWander Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateWander)FindObjectOfType(typeof(StateWander)) as StateWander;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Wander();
        Debug.Log("Wander");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
    }
    public Vector2 Wander()
    {

        float JitterThisTimeSlice = AgentBehavior.Instance.m_dWanderJitter * Time.deltaTime;

        AgentBehavior.Instance.m_vWanderTarget += new Vector2(Random.Range(-1.0f, 1.0f) * JitterThisTimeSlice, Random.Range(-1.0f, 1.0f) * JitterThisTimeSlice);

        AgentBehavior.Instance.m_vWanderTarget.Normalize();

        AgentBehavior.Instance.m_vWanderTarget *= AgentBehavior.Instance.m_dWanderRadius;

        Vector2 _target = AgentBehavior.Instance.m_vWanderTarget + new Vector2(AgentBehavior.Instance.m_dWanderDistance, 0);

        Debug.DrawLine((Vector2)AgentBehavior.Instance.transform.position, (Vector2)AgentBehavior.Instance.transform.position + _target, Color.red);

        return StateSeek.Instance.Seek(_target);

    }

}
public class StateOffsetPursuit : MonoBehaviour, IState
{
    public static StateOffsetPursuit instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateOffsetPursuit Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateOffsetPursuit)FindObjectOfType(typeof(StateOffsetPursuit)) as StateOffsetPursuit;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Offset1").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Offset2").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Offset3").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += OffsetPursuit(AgentBehavior.Instance.leader, AgentBehavior.Instance.offsetPosition);
        Debug.Log("OffsetPursuit");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Offset1").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Offset2").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Offset3").gameObject.SetActive(false);
    }
    public Vector2 OffsetPursuit(GameObject leader, Vector2 offset)
    {

        GameObject offSetPosition = new GameObject();
        offSetPosition.transform.position = leader.transform.position;
        offSetPosition.transform.rotation = leader.transform.rotation;
        offSetPosition.transform.Translate(offset);

        Vector2 saveOffsetPos = offSetPosition.transform.position;

        Vector2 toOffset = offSetPosition.transform.position - AgentBehavior.Instance.transform.position;

        float LookAHeadTime = toOffset.magnitude / (AgentBehavior.Instance.maxSpeed + leader.GetComponent<Rigidbody>().velocity.magnitude);

        MonoBehaviour.DestroyObject(offSetPosition);
        return StateArrive.Instance.Arrive(saveOffsetPos + (Vector2)leader.GetComponent<Rigidbody>().velocity * LookAHeadTime);

    }
}
public class StateInterpose : MonoBehaviour, IState
{
    public static StateInterpose instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateInterpose Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateInterpose)FindObjectOfType(typeof(StateInterpose)) as StateInterpose;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("interpose").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Evader").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += InterPose(AgentBehavior.Instance.interPoseAgentX, AgentBehavior.Instance.interPoseAgentY);
        Debug.Log("Interpose");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("interpose").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Evader").gameObject.SetActive(false);
    }
    public Vector2 InterPose(GameObject agentA, GameObject agentB)
    {
        Vector2 midPoint = (agentA.transform.position + agentB.transform.position) / 2.0f;

        float timeToReachMidPoint = Vector2.Distance(AgentBehavior.Instance.transform.position, midPoint) / AgentBehavior.Instance.maxSpeed;

        Vector2 aPos = agentA.transform.position + agentA.GetComponent<Rigidbody>().velocity * timeToReachMidPoint;
        Vector2 bPos = agentB.transform.position + agentB.GetComponent<Rigidbody>().velocity * timeToReachMidPoint;

        midPoint = (aPos + bPos) / 2.0f;

        return StateArrive.Instance.Arrive(midPoint);
    }
}
public class StateHide : MonoBehaviour, IState
{
    public static StateHide instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static StateHide Instance
    {
        get
        {
            if (!instance)
            {
                instance = (StateHide)FindObjectOfType(typeof(StateHide)) as StateHide;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }
    public void OperateEnter()
    {

        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock1").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock2").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock3").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock4").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock5").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("HideBlock6").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Hide1").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Hide2").gameObject.SetActive(true);
        GameObject.Find("Objects").transform.Find("Hide3").gameObject.SetActive(true);
    }
    public void OperateUpdate()
    {
        AgentBehavior.Instance.steering += Hide(AgentBehavior.Instance.Hunter);
        Debug.Log("Hide");
    }
    public void OperateExit()
    {
        GameObject.Find("Objects").transform.Find("Agent").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock1").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock2").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock3").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock4").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock5").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("HideBlock6").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Hide1").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Hide2").gameObject.SetActive(false);
        GameObject.Find("Objects").transform.Find("Hide3").gameObject.SetActive(false);
    }
    public Vector2 Hide(GameObject hunter)
    {
        float distToClosest = float.MaxValue;
        Vector2 bestHidingSpot = Vector2.zero;

        GameObject closet;

        foreach (var circleObject in AgentBehavior.Instance.circles)
        {
            float circleRadius = circleObject.GetComponent<CapsuleCollider>().radius;
            Vector2 hidingSpot = GetHidingPosition(circleObject.transform.position, circleRadius, hunter.transform.position);

            float dist = Vector2.Distance(hidingSpot, AgentBehavior.Instance.transform.position);
            if (dist < distToClosest)
            {
                distToClosest = dist;

                bestHidingSpot = hidingSpot;

                closet = circleObject;
            }

        }
        if (distToClosest == float.MaxValue)
        {
            return StateEvade.Instance.Evade(hunter);
        }

        return StateArrive.Instance.Arrive(bestHidingSpot);
    }

    Vector2 GetHidingPosition(Vector2 posOb, float radiusOb, Vector2 posHunter)
    {
        float distanceFromBoundary = 0.3f;
        float distAway = radiusOb + distanceFromBoundary;

        Vector2 toOb = posOb - posHunter;
        toOb.Normalize();

        return (toOb * distAway) + posOb;
    }
}
