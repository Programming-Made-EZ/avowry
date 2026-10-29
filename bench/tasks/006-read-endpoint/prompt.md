Read this Avowry endpoint and add comments that explain its input, output, steps, and spec without changing behavior. Return only valid Avowry source.

```avowry
module Echo

endpoint EchoText : POST /echo
  input
    text: Text
  output
    text: Text
  steps
    respond with { text: input.text }
  spec "echoes text"
    given input { text: "hello" }
    when handled
    then responds { text: "hello" }
```
