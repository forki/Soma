﻿//----------------------------------------------------------------------------
//
// Copyright (c) 2011 The Soma Team. 
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.txt file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------
namespace Soma.Core.IT

open System
open System.Configuration
open System.Data
open System.Data.Common
open System.Transactions
open NUnit.Framework
open Soma.Core

module DeleteTest = 
    [<Test>]
    let ``delete : no id``() = 
        use ts = new TransactionScope()
        try 
            MsSql.delete { NoId.Name = "aaa"
                           VersionNo = 0 }
            |> ignore
            fail()
        with
        | :? InvalidOperationException as ex -> 
            assert_true <| ex.Message.StartsWith "[SOMA4005]"
            printfn "%A" ex
        | ex -> fail ex
    
    [<Test>]
    let ``delete : no version``() = 
        use ts = new TransactionScope()
        MsSql.delete { NoVersion.Id = 1
                       Name = "aaa" }
    
    [<Test>]
    let ``delete : incremented version``() = 
        use ts = new TransactionScope()
        let department = MsSql.find<Department> [ 1 ]
        MsSql.delete department
    
    [<Test>]
    let ``delete : computed version``() = 
        use ts = new TransactionScope()
        
        let department = 
            MsSql.insert { AddressId = 0
                           Street = "hoge"
                           VersionNo = Array.empty }
        MsSql.delete department
    
    [<Test>]
    let ``delete : incremented version : optimistic lock confliction``() = 
        use ts = new TransactionScope()
        
        let department = 
            { DepartmentId = 1
              DepartmentName = "hoge"
              VersionNo = -1 }
        try 
            MsSql.delete department |> ignore
            fail()
        with
        | OptimisticLockException _ as ex -> printfn "%s" (string ex)
        | ex -> fail ex
    
    [<Test>]
    let ``delete : computed version : optimistic lock confliction``() = 
        use ts = new TransactionScope()
        
        let address = 
            { AddressId = 1
              Street = "hoge"
              VersionNo = Array.empty }
        try 
            MsSql.delete address |> ignore
            fail()
        with
        | OptimisticLockException _ as ex -> printfn "%s" (string ex)
        | ex -> fail ex
    
    [<Test>]
    let deleteIgnoreVersion() = 
        use ts = new TransactionScope()
        
        let department = 
            { DepartmentId = 1
              DepartmentName = "aaa"
              VersionNo = -1 }
        MsSql.deleteWithOpt department (DeleteOpt(IgnoreVersion = true))
    
    [<Test>]
    let ``deleteIgnoreVersion : no affected row``() = 
        use ts = new TransactionScope()
        
        let department = 
            { DepartmentId = 0
              DepartmentName = "aaa"
              VersionNo = -1 }
        try 
            MsSql.deleteWithOpt department (DeleteOpt(IgnoreVersion = true)) |> ignore
            fail()
        with
        | NoAffectedRowException _ as ex -> printfn "%s" (string ex)
        | ex -> fail ex
