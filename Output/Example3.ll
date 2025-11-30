; ModuleID = 'ControlDemo'
source_filename = "ControlDemo"

@n = global i32 0
@result = global i32 0

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define i32 @Outer(i32) {
entry:
  %retval = alloca i32
  %x = alloca i32
  store i32 %0, i32* %x
  %acc = alloca i32

return:                                           ; No predecessors!
}

define i32 @Inner(i32) {
entry:
  %retval = alloca i32
  %y = alloca i32
  store i32 %0, i32* %y
  %load = load i32, i32* %y
  %mul = mul i32 %load, 2
  store i32 %mul, i32* %retval
  br label %return
  store i32 0, i32* %acc
  br label %while.cond

return:                                           ; preds = %while.body, %entry
}

define i32 @main() {
entry:
  store i32 5, i32* @n
  %load = load i32, i32* @n
  %cmpge = icmp sge i32 %load, 5
  br i1 %cmpge, label %if.then, label %if.else

if.then:                                          ; preds = %entry
  %load1 = load i32, i32* @n
  %call = call i32 (i32) @Outer(i32 %load1)
  store i32 (i32) %call, i32* @result
  store i32 0, i32* @result
  ret i32 0

if.merge:                                         ; No predecessors!

if.else:                                          ; preds = %entry
}
