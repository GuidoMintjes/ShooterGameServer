using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer {
    public class Player {

        public int id;
        public string userName;

        public Vector3 position;
        public Quaternion rotation;


        public Player(int _id, string _name, Vector3 spawnPos) {

            id = _id;
            userName = _name;
            position = spawnPos;
            rotation = Quaternion.Identity;
        }
    }
}
