; ModuleID = 'ArrayDemo'
source_filename = "ArrayDemo"

@mat = global [10 x [10 x i32]] zeroinitializer
@res = global [10 x [10 x i32]] zeroinitializer

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define void @FillMatrix([10 x [10 x i32]]) {
entry:
  %a = alloca [10 x [10 x i32]]
  store [10 x [10 x i32]] %0, [10 x [10 x i32]]* %a
  %i = alloca i32
  %j = alloca i32
  %load = load i32, i32* %i
  %mul = mul i32 %load, 10
  %load1 = load i32, i32* %j
  %add = add i32 %mul, %load1
  %load2 = load i32, i32* %i
  %load3 = load i32, i32* %j
  %arrayptr = getelementptr [10 x [10 x i32]], [10 x [10 x i32]]* %a, i32 0, i32 %load2, i32 %load3
  store i32 %add, i32* %arrayptr
  ret void
}

define i32 @SumMatrix([10 x [10 x i32]]) {
entry:
  %retval = alloca i32
  %a = alloca [10 x [10 x i32]]
  store [10 x [10 x i32]] %0, [10 x [10 x i32]]* %a
  %i = alloca i32
  %j = alloca i32
  %sum = alloca i32
  store i32 0, i32* %sum
  %load = load i32, i32* %sum
  %load1 = load i32, i32* %i
  %load2 = load i32, i32* %j
  %arrayptr = getelementptr [10 x [10 x i32]], [10 x [10 x i32]]* %a, i32 0, i32 %load1, i32 %load2
  %load3 = load i32, i32* %arrayptr
  %add = add i32 %load, %load3
  store i32 %add, i32* %sum
  %load4 = load i32, i32* %sum
  store i32 %load4, i32* %retval
  br label %return
  %load5 = load [10 x [10 x i32]], [10 x [10 x i32]]* @mat
  %1 = call void ([10 x [10 x i32]]) @FillMatrix([10 x [10 x i32]] %load5)
  %load6 = load [10 x [10 x i32]], [10 x [10 x i32]]* @mat
  store [10 x [10 x i32]] %load6, [10 x [10 x i32]]* @res

return:                                           ; preds = %entry
}

define i32 @main() {
entry:
  %load = load [10 x [10 x i32]], [10 x [10 x i32]]* @mat
  %0 = call void ([10 x [10 x i32]]) @FillMatrix([10 x [10 x i32]] %load)
  %load1 = load [10 x [10 x i32]], [10 x [10 x i32]]* @mat
  store [10 x [10 x i32]] %load1, [10 x [10 x i32]]* @res
  ret i32 0
}
