using UnityEngine;
using System.Collections;
 
// Требовать Rigidbody и LineRenderer объекта для облегчения сборке
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(LineRenderer))]
 
public class LineRender : MonoBehaviour
{
 
	public Transform target;
	public float resolution = 0.5F;							  //  Устанавливает количество стыков есть в канате (1 = 1 соединение для каждого 1 единица)
	public float ropeDrag = 0.1F;								 //  Задает каждому суставов Drag
	public float ropeMass = 0.1F;							//  Sets each joints Mass
	public float ropeColRadius = 0.5F;					//  Задает каждому суставов массу
	//public float ropeBreakForce = 25.0F;					 //-------------- TODO (Hopefully will break the rope in half...
	private Vector3[] segmentPos;			//  Не шутите! Это для линии Renderer ведения и настроить позиции игровых объектов
	private GameObject[] joints;			// Не шутите! Это фактическое совместное объектов, которые будут созданы автоматически
	private LineRenderer line;							//  Не шутите!	 Переменная линии визуализации настроен, когда его назначен новый компонент
	private int segments = 0;					//  Не шутите!	Количество сегментов рассчитывается с вашего расстояние * разрешение
	private bool rope = false;						 //  Не шутите!	Это позволит нам избежать ошибки из вашего окна отладки! Сохраняет веревку от оказания когда она не существует ...
 
	//Joint Settings
	public Vector3 swingAxis = new Vector3 (1, 1, 1);				 //  Наборы которых оси характер совместной будет качаться на (1 ось лучше для 2D, 2-3 оси лучше всего подходит для 3D (по умолчанию = 3 оси))
	public float lowTwistLimit = -100.0F;					//  Нижняя граница вокруг основной оси характера сустава.
	public float highTwistLimit = 100.0F;					//  Верхняя граница вокруг основной оси характера сустава.
	public float swing1Limit = 20.0F;					//	Предельный вокруг основной оси характера совместной начиная с инициализации точки.
 
	void Awake ()
	{
		BuildRope ();
	}
 
	void Update ()
	{
		// Put rope control here!
 
 
		//Уничтожить Rope Test (Пример того, как можно использовать веревку динамически)
		if (rope && Input.GetMouseButtonDown (0)) {
			DestroyRope ();	
		}	
		if (!rope && Input.GetMouseButtonDown (1)) {
			BuildRope ();
		}
	}

	void LateUpdate ()
	{
		//Есть ли веревка существует? Если это так, обновите свои позиции
		if (rope) {
			for (int i=0; i<segments; i++) {
				if (i == 0) {
					line.SetPosition (i, transform.position);
				} else if (i == segments - 1) {
					line.SetPosition (i, target.transform.position);	
				} else {
					line.SetPosition (i, joints [i].transform.position);
				}
			}
			line.enabled = true;
		} else {
			line.enabled = false;	
		}
	}
 
	void BuildRope ()
	{
		line = gameObject.GetComponent<LineRenderer> ();
 
		// Найти количество сегментов в зависимости от расстояния и разрешения
		// Пример: [разрешение 1,0 = 1 совместных на единицу расстояния]
		segments = (int)(Vector3.Distance (transform.position, target.position) * resolution);
		line.SetVertexCount (segments);
		segmentPos = new Vector3[segments];
		joints = new GameObject[segments];
		segmentPos [0] = transform.position;
		segmentPos [segments - 1] = target.position;
 
		// Найти расстояние между каждым сегментом
		var segs = segments - 1;
		var seperation = ((target.position - transform.position) / segs);
 
		for (int s=1; s < segments; s++) {
			// Найти каждого сегмента положение с помощью склону сверху
			Vector3 vector = (seperation * s) + transform.position;	
			segmentPos [s] = vector;
 
			//Добавить физики в сегментах
			AddJointPhysics (s);
		}
 
		// Прикрепить суставов целевой объект и привяжите его к этому объекту
		CharacterJoint end = target.gameObject.AddComponent<CharacterJoint> ();
		end.connectedBody = joints [joints.Length - 1].transform.rigidbody;
		end.swingAxis = swingAxis;
		SoftJointLimit limit_setter = end.lowTwistLimit;
		limit_setter.limit = lowTwistLimit;
		end.lowTwistLimit = limit_setter;
		limit_setter = end.highTwistLimit;
		limit_setter.limit = highTwistLimit;
		end.highTwistLimit = limit_setter;
		limit_setter = end.swing1Limit;
		limit_setter.limit = swing1Limit;
		end.swing1Limit = limit_setter;
		target.parent = transform;
 
		// Веревка = правда, веревку сейчас существует на сцене!
		rope = true;
	}
 
	void AddJointPhysics (int n)
	{
		joints [n] = new GameObject ("Joint_" + n);
		joints [n].transform.parent = transform;
		Rigidbody rigid = joints [n].AddComponent<Rigidbody> ();
		SphereCollider col = joints [n].AddComponent<SphereCollider> ();
		CharacterJoint ph = joints [n].AddComponent<CharacterJoint> ();
		ph.swingAxis = swingAxis;
		SoftJointLimit limit_setter = ph.lowTwistLimit;
		limit_setter.limit = lowTwistLimit;
		ph.lowTwistLimit = limit_setter;
		limit_setter = ph.highTwistLimit;
		limit_setter.limit = highTwistLimit;
		ph.highTwistLimit = limit_setter;
		limit_setter = ph.swing1Limit;
		limit_setter.limit = swing1Limit;
		ph.swing1Limit = limit_setter;
		//ph.breakForce = ropeBreakForce; <--------------- TODO
 
		joints [n].transform.position = segmentPos [n];
 
		rigid.drag = ropeDrag;
		rigid.mass = ropeMass;
		col.radius = ropeColRadius;
 
		if (n == 1) {		
			ph.connectedBody = transform.rigidbody;
		} else {
			ph.connectedBody = joints [n - 1].rigidbody;	
		}
 
	}
 
	void DestroyRope ()
	{
		// Прекратить оказание Rope затем уничтожить все его компоненты
		rope = false;
		for (int dj=0; dj<joints.Length-1; dj++) {
			Destroy (joints [dj]);	
		}
 
		segmentPos = new Vector3[0];
		joints = new GameObject[0];
		segments = 0;
	}
}