/**********************************************************************************************************************
 * Copyright WuGanhao. All Rights Reserved.
 *
 * This software is the confidential and proprietary information of
 * WuGanhao. ("Confidential Information"). You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered
 * into with WuGanhao.
 *********************************************************************************************************************/

using System;

namespace WuGanhao.CommandLineParser {
    /// <summary>
    /// Command line exception
    /// </summary>
    public class CommandLineException: SystemException {

        /// <summary>
        /// Create new instance of CommandLineException
        /// </summary>
        /// <param name="message"></param>
        public CommandLineException(string message): base(message) {
        }
    }
}
