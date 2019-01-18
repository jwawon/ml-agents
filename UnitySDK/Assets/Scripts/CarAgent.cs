using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    private Rigidbody carRigidbody; //자동차의 rigidbody
    public Transform pivotTransform; //TrainingEnv의 기준좌표
    public Transform goal; //goal의 위치를 확인
    public float moveForce =1f; //자동차 이동힘
    private bool isGoal = false; //Goal에 도착했는지
    private bool isDead = false; //사망 상태 확인
    void Awake() {
        carRigidbody = GetComponent<Rigidbody>();    
    }

    void ResetGoal(){
        isGoal = false;

    }

    //Agent가 특정 이벤트를 만났을때 자동실행되는 함수
    //Agent가 Reset되면 실행되는 함수
    public override void AgentReset(){
        Vector3 initPos = new Vector3(0f, 0.5f, -4f);
        transform.rotation = Quaternion.Euler(0f,0f,0f);
        transform.position = initPos + pivotTransform.position;
        
        isDead = false;
        carRigidbody.velocity = Vector3.zero;

        ResetGoal();
    }

    //Agent가 주변을 관측하는 함수
    //기록된 vector 공간의 정보를 통해서 판단하기 위한 함수
    public override void CollectObservations(){
        Vector3 distanceToGoal = goal.position - transform.position;
        //관측정보를 저장, 위아래는 움직이지 않아서 y 값은 필요없다.
        AddVectorObs(distanceToGoal.x);
        AddVectorObs(distanceToGoal.z);

        Vector3 relativePos = transform.position - pivotTransform.position;
        AddVectorObs(relativePos.x);
        AddVectorObs(relativePos.z);

        AddVectorObs(carRigidbody.velocity.x);
        AddVectorObs(carRigidbody.velocity.z);

    }
    //Agent가 선택에 의한 행동을 취하는 함수
    //Unity의 fixedUpdate와 동일한 frame으로 실행된다 (0.02초)
    public override void AgentAction(float[] vectorAction, string textAction){
        //Agent가 움직이지 않을때 주는 벌점
        //벌점이 너무 과하면 빨리 죽는게 더 보상이 크다고 생각한다.
        AddReward(-0.001f);
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        float horizontalInput = vectorAction[0];
        // Debug.Log("Horizontal: " + horizontalInput);
        float verticalInput = vectorAction[1];
        // Debug.Log("Vertical: " + verticalInput);
        dirToGo = transform.forward * Mathf.Clamp(verticalInput, -1f, 1f);
        rotateDir = transform.up * Mathf.Clamp(horizontalInput, -1f, 1f);
        transform.Translate(dirToGo);
        transform.Rotate(rotateDir, Time.deltaTime * 10f);

        // carRigidbody.AddForce(horizontalInput* moveForce,0,verticalInput*moveForce, ForceMode.Acceleration);

        //Action에 의한 결과, 보상과 처벌을 같이 넣어줘야한다.
        if(isGoal){
            Debug.Log("Goal!!");
            AddReward(1.0f);
            ResetGoal();
            Done();
        }
        else if(isDead){
            AddReward(-1.0f);
            //현재까지 결과와 보상등의 정보를 Tensorflow에 보내고 멈추고 훈련이 다시 시작
            Done();
        }        

    }
    //Dead zone에 부딪혔는지, Goal에 도착했는지를 확인하는 함수
    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Dead")){
            isDead = true;
        }
        else if(other.CompareTag("goal")){
            isGoal = true;
        }
    }
}
