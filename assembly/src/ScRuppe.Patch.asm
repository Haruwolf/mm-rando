;==================================================================================================
; When the rupee is picked up.
;==================================================================================================

.headersize G_EN_SC_RUPPE_DELTA

; Replaces:
;   mtc1    r0, f4
;   sh      r0, 0x0194 (a2)
;   addiu   a1, r0, 0x4803
;   swc1    f4, 0x0074 (a2)
.org 0x80BD6AB0
    jal     ScRuppe_GiveItem_Hook
    sw      a2, 0x0018 (sp)
    bnez    v0, 0x80BD6AE8
    lw      a2, 0x0018 (sp)
