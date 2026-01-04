; ModuleID = 'ArrayDemo'
source_filename = "ArrayDemo"
target triple = "x86_64-pc-windows-msvc"

@mat = global [2 x [2 x i64]] zeroinitializer
@res = global [2 x [2 x i64]] zeroinitializer
@.fmt = private unnamed_addr constant [5 x i8] c"%lld\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define void @FillMatrix([2 x [2 x i64]]*) {
entry:
  %i = alloca i64
  %j = alloca i64
  store i64 0, i64* %i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i1 = load i64, i64* %i
  %for.cmp = icmp sle i64 %i1, 1
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* %j
  br label %for.cond2

for.inc:                                          ; preds = %for.end5
  %i14 = load i64, i64* %i
  %for.inc15 = add i64 %i14, 1
  store i64 %for.inc15, i64* %i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  ret void

for.cond2:                                        ; preds = %for.inc4, %for.body
  %j6 = load i64, i64* %j
  %for.cmp7 = icmp sle i64 %j6, 1
  br i1 %for.cmp7, label %for.body3, label %for.end5

for.body3:                                        ; preds = %for.cond2
  %i8 = load i64, i64* %i
  %j9 = load i64, i64* %j
  %arrayidx = getelementptr inbounds [2 x [2 x i64]], [2 x [2 x i64]]* %0, i64 0, i64 %i8, i64 %j9
  %i10 = load i64, i64* %i
  %mul = mul i64 %i10, 10
  %j11 = load i64, i64* %j
  %add = add i64 %mul, %j11
  store i64 %add, i64* %arrayidx
  br label %for.inc4

for.inc4:                                         ; preds = %for.body3
  %j12 = load i64, i64* %j
  %for.inc13 = add i64 %j12, 1
  store i64 %for.inc13, i64* %j
  br label %for.cond2

for.end5:                                         ; preds = %for.cond2
  br label %for.inc
}

define i64 @SumMatrix([2 x [2 x i64]]*) {
entry:
  %i = alloca i64
  %j = alloca i64
  %sum = alloca i64
  store i64 0, i64* %sum
  store i64 0, i64* %i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i1 = load i64, i64* %i
  %for.cmp = icmp sle i64 %i1, 1
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* %j
  br label %for.cond2

for.inc:                                          ; preds = %for.end5
  %i13 = load i64, i64* %i
  %for.inc14 = add i64 %i13, 1
  store i64 %for.inc14, i64* %i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %sum15 = load i64, i64* %sum
  ret i64 %sum15

for.cond2:                                        ; preds = %for.inc4, %for.body
  %j6 = load i64, i64* %j
  %for.cmp7 = icmp sle i64 %j6, 1
  br i1 %for.cmp7, label %for.body3, label %for.end5

for.body3:                                        ; preds = %for.cond2
  %sum8 = load i64, i64* %sum
  %i9 = load i64, i64* %i
  %j10 = load i64, i64* %j
  %arrayidx = getelementptr inbounds [2 x [2 x i64]], [2 x [2 x i64]]* %0, i64 0, i64 %i9, i64 %j10
  %elem = load i64, i64* %arrayidx
  %add = add i64 %sum8, %elem
  store i64 %add, i64* %sum
  br label %for.inc4

for.inc4:                                         ; preds = %for.body3
  %j11 = load i64, i64* %j
  %for.inc12 = add i64 %j11, 1
  store i64 %for.inc12, i64* %j
  br label %for.cond2

for.end5:                                         ; preds = %for.cond2
  br label %for.inc
}

define i32 @main() {
entry:
  call void @FillMatrix([2 x [2 x i64]]* @mat)
  %mat = load [2 x [2 x i64]], [2 x [2 x i64]]* @mat
  %0 = call i8* @memcpy(i8* bitcast ([2 x [2 x i64]]* @res to i8*), i8* bitcast ([2 x [2 x i64]]* @mat to i8*), i64 32)
  %1 = call i64 @SumMatrix([2 x [2 x i64]]* @res)
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt, i32 0, i32 0), i64 %1)
  ret i32 0
}

declare i8* @memcpy(i8*, i8*, i64)
