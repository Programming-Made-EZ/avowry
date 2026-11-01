What the architect reads and writes at the slice step.

- Rule: The slice numbers the acceptance criteria so that each red can name exactly one. Quote: "The slice record numbers the card's acceptance criteria.". See [D157](../../decisions/process-and-staffing.md#d157).
- Rule: A card is shippable as is only with one concern, at most three increments and at most three criteria. Quote: "at most three acceptance criteria (scored from the issue body), at most three increments and one concern". See [D136](../../decisions/process-and-staffing.md#d136).
- Rule: A slice file is 15360 bytes or fewer and ends with a Size line measured with head -n -1; the commit body repeats the number. Quote: "Every slice file committed for a card is 15360 bytes or fewer.". See [D168](../../decisions/process-and-staffing.md#d168).
- Rule: The slice carries an assumption line with a probe for any card that changes a rule, the ranking or the checker. Quote: "A card whose slice changes a rule, the ranking or order of cards, or the checker adds one line to the slice". See [D166](../../decisions/process-and-staffing.md#d166).
- Rule: The slice carries a hostile-input line for each criterion that takes a list or map of names. Quote: "A criterion that takes a list, set or map of names (seats, backends, models, rankings) gets one slice line". See [D167](../../decisions/process-and-staffing.md#d167).
