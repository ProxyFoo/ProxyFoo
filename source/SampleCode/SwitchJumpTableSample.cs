// Copyright © 2014, Silverlake Software LLC.  All Rights Reserved.
// SILVERLAKE SOFTWARE LLC CONFIDENTIAL INFORMATION
//
// Created by Jamie da Silva on 3/31/2014 2:05 PM

using System;

namespace SampleCode
{
    public static class SwitchJumpTableSample
    {
        public static int Get(int value)
        {
            switch (value)
            {
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 4;
            }

            return 0;
        }
    }
}